using System.Text;
using consumindoIA.Domain;
using Learnly.Services.Interfaces;
using Newtonsoft.Json;

namespace Learnly.Services.IAService
{
    public class IAService : IIAService
    {
        private readonly HttpClient _httpClient;

        public IAService(string apiKey)
        {
            _httpClient = new();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<string> GerarFeedbackAsync(Simulado simulado)
        {
            try
            {
                var resumoIA = GerarResumoIA(simulado);
                var jsonResumo = JsonConvert.SerializeObject(resumoIA, Formatting.Indented);
                var mensagens = new List<Message>
                {
                    new Message
                    {
                        role = "system",
                        content = "Você é um tutor do ENEM que gera feedbacks curtos, claros e objetivos."
                    },
                    new Message
                    {
                        role = "user",
                        content = $@"
                Dados do simulado:
                {jsonResumo}

                Gere um feedback em no máximo:
                - 1 parágrafo de panorama geral
                - 3 bullets de padrões de erro
                - 3 bullets de recomendações de estudo

                Importante: Formate a mensagem para que a leitura seja fluida e rápida
                "
                    }
                };

                var request = new ChatRequest
                {
                    model = "moonshotai/kimi-k2-instruct",
                    messages = mensagens,
                    temperature = 0.5
                };

                var json = JsonConvert.SerializeObject(request);
                var corpo = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    "https://api.groq.com/openai/v1/chat/completions",
                    corpo
                );

                if (!response.IsSuccessStatusCode)
                    return "Erro ao gerar feedback.";

                var respostaJson = await response.Content.ReadAsStringAsync();
                var resposta = JsonConvert.DeserializeObject<ChatResponse>(respostaJson);

                return resposta?.choices?[0]?.message?.content ?? "Erro ao interpretar resposta da IA.";
            }
            catch (Exception ex)
            {
                return $"Erro IA: {ex.Message}";
            }
        }

        private SimuladoIAResumo GerarResumoIA(Simulado simulado)
        {
            var resumo = new SimuladoIAResumo
            {
                SimuladoId = simulado.SimuladoId,
                NotaFinal = simulado.NotaFinal,
                Disciplinas = new Dictionary<string, DisciplinaIAResumo>()
            };

            foreach (var simuladoQuestao in simulado.Questoes)
            {
                var questao = simuladoQuestao.Questao;
                if (questao == null) continue;

                var resposta = simulado.Respostas
                    .FirstOrDefault(r => r.QuestaoId == questao.QuestaoId);

                if (resposta?.Alternativa == null) continue;

                var disciplina = questao.Disciplina ?? "Geral";
                var habilidade = DetectarHabilidade(questao);

                if (!resumo.Disciplinas.ContainsKey(disciplina))
                {
                    resumo.Disciplinas[disciplina] = new DisciplinaIAResumo
                    {
                        Total = 0,
                        Acertos = 0,
                        ErrosPorHabilidade = new Dictionary<string, int>()
                    };
                }

                var bloco = resumo.Disciplinas[disciplina];
                bloco.Total++;

                bool acertou = resposta.Alternativa.Letra == questao.AlternativaCorreta;

                if (acertou)
                {
                    bloco.Acertos++;
                }
                else
                {
                    if (!bloco.ErrosPorHabilidade.ContainsKey(habilidade))
                        bloco.ErrosPorHabilidade[habilidade] = 0;

                    bloco.ErrosPorHabilidade[habilidade]++;
                }
            }

            return resumo;
        }

        private string DetectarHabilidade(Questao q)
        {
            var texto = ((q.Titulo ?? "") + " " + (q.IntroducaoAlternativa ?? "") + " " + (q.Contexto ?? "")).ToLower();

            if (texto.Contains("pronome")) return "pronomes";
            if (texto.Contains("figura") || texto.Contains("linguagem")) return "figuras de linguagem";
            if (texto.Contains("interpreta") || texto.Contains("sentido")) return "interpretação";
            if (texto.Contains("texto") || texto.Contains("poema")) return "compreensão textual";
            if (texto.Contains("gramática") || texto.Contains("sintaxe")) return "gramática";

            return "geral";
        }
    }
}
