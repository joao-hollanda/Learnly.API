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
                    await Task.Delay(TimeSpan.FromSeconds((i + 1) * 8));
                    continue;
                }

                throw new Exception($"Groq retornou {(int)httpResponse.StatusCode}: {respostaJson}");
            }

            throw new Exception("Groq: limite de tentativas atingido após rate limit.");
        }
    }
}