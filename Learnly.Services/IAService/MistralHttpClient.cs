using System.Text;
using consumindoIA.Domain;
using Newtonsoft.Json;

namespace Learnly.Services.IAService
{
    public class MistralHttpClient
    {
        private readonly HttpClient _httpClient;

        public MistralHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> EnviarAsync(object requestBody)
        {
            var json = JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            for (int i = 0; i < 3; i++)
            {
                var corpo = new StringContent(json, Encoding.UTF8, "application/json");
                var httpResponse = await _httpClient.PostAsync("https://api.mistral.ai/v1/chat/completions", corpo);
                var respostaJson = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var resposta = JsonConvert.DeserializeObject<ChatResponse>(respostaJson);
                    return resposta?.choices?[0]?.message?.content;
                }

                if ((int)httpResponse.StatusCode == 429)
                {
                    if (i == 2)
                        throw new Exception("A correção por IA está sobrecarregada no momento. Aguarde alguns minutos e tente novamente.");

                    await Task.Delay(TimeSpan.FromSeconds(8 * (i + 1)));
                    continue;
                }

                throw new Exception($"Mistral retornou {(int)httpResponse.StatusCode}: {respostaJson}");
            }

            throw new Exception("Mistral: limite de tentativas atingido após rate limit.");
        }
    }
}
