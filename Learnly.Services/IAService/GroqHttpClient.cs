using System.Text;
using System.Text.RegularExpressions;
using consumindoIA.Domain;
using Learnly.Domain.Entities;
using Newtonsoft.Json;

namespace Learnly.Services.IAService
{
    public class GroqHttpClient
    {
        private readonly HttpClient _httpClient;

        public GroqHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> EnviarAsync(object requestBody)
        {
            var response = await PostAsync(requestBody);
            return response?.choices?[0]?.message?.content;
        }

        public async Task<Message?> EnviarCompletaAsync(object requestBody)
        {
            var response = await PostAsync(requestBody);
            var message = response?.choices?[0]?.message;

            if (message?.content != null)
            {
                message.content = Regex.Replace(
                    message.content,
                    @"<think>[\s\S]*?</think>",
                    ""
                ).Trim();
            }

            return message;
        }

        private async Task<ChatResponse?> PostAsync(object requestBody)
        {
            var json = JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            for (int i = 0; i < 3; i++)
            {
                var corpo = new StringContent(json, Encoding.UTF8, "application/json");
                var httpResponse = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", corpo);
                var respostaJson = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<ChatResponse>(respostaJson);

                if ((int)httpResponse.StatusCode == 429)
                {
                    if (i == 2)
                        throw new Exception("O MentorIA está sobrecarregado no momento. Aguarde alguns segundos e tente novamente.");

                    var segundos = 8 * (i + 1);
                    try
                    {
                        var erro = JsonConvert.DeserializeObject<dynamic>(respostaJson);
                        string mensagemErro = erro?.error?.message?.ToString() ?? "";
                        var match = Regex.Match(mensagemErro, @"try again in (\d+(?:\.\d+)?)s");
                        if (match.Success && double.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double seg))
                            segundos = (int)Math.Ceiling(seg) + 1;
                    }
                    catch { }

                    await Task.Delay(TimeSpan.FromSeconds(segundos));
                    continue;
                }

                throw new Exception($"Groq retornou {(int)httpResponse.StatusCode}: {respostaJson}");
            }

            throw new Exception("Groq: limite de tentativas atingido após rate limit.");
        }
    }
}