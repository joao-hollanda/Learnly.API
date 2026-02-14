using System.Text;
using System.Text.Json;
using consumindoIA.Domain;
using Learnly.Domain.Entities;
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
                        content = @"Você é um Mentor de Performance para o ENEM. 
                        Seu estilo de escrita é direto, técnico e encorajador. 
                        Em vez de listas simples, você cria conexões entre o erro do aluno e o que ele precisa fazer para subir de nível."
                    },
                    new Message
                    {
                        role = "user",
                        content = $@"
                        Dados do simulado:
                        {jsonResumo}

                        Gere um feedback estruturado da seguinte forma (sem usar bullets):

                        1. **Análise de Proficiência (Onde você está)**: 
                        Um parágrafo curto analisando a coerência do desempenho. Foque se o aluno está perdendo pontos em conteúdos base (TRI) ou se o problema é fôlego de prova.

                        2. **Gargalos Identificados (O que te trava)**: 
                        Um texto fluido que conecte os erros a um padrão. Exemplo: 'Notei que o tempo gasto em questões de Exatas está prejudicando seu desempenho em Linguagens, indicando uma necessidade de priorização.'

                        3. **Roteiro de Evolução (O próximo passo)**: 
                        Uma instrução clara e prática do que ele deve estudar amanhã e como deve mudar a estratégia de resolução na próxima prova.

                        Use negrito para termos técnicos e métricas importantes. Evite listas, prefira um texto que pareça uma conversa de mentoria rápida, evitando extender muito (Opte por 3-4 linhas por tópico)."
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

        public async Task<PlanoEstudo> GerarPlanoIA(CriarPlanoIADTO plano)
        {

            var messages = new[]
            {
        new { role = "system", content = $@"- Materias devem ser relevantes para '{plano.Objetivo}'

- Cada matéria 4-6 tópicos

- Datas 2026

- JSON válido, apenas o objeto, sem explicações

- O plano começa no dia {plano.DataInicio} e termina no {plano.DataFim}

- O usuário tem {plano.HorasPorSemana} horas por semana para estudar
" },
        new { role = "user", content = $@"Gere um plano de estudos '{plano.Titulo}' seguindo rigorosamente este schema:
{{
  ""HorasPorSemana"": {plano.HorasPorSemana},
  ""Ativo"": true,
  ""UsuarioId"": {plano.UsuarioId},
  ""PlanoMaterias"": [
    {{
      ""Materia"": {{
        ""Nome"": ""Nome da Matéria"",
        ""GeradaPorIA"": true,
        ""GeradaPorIA"": true,
        ""Cor"": ""Cor em formato HEX (#RRGGBB)""
      ""HorasTotais"": calcule conforme as datas e carga horária semanal. Priorize o que achar mais importante ou dificil
      ""Topicos"": [""Tópico 1"", ""Tópico 2""]
    }}
  ]
}}" }
    };

            var requestBody = new
            {
                model = "llama-3.3-70b-versatile",
                messages = messages,
                temperature = 0.2,
                response_format = new { type = "json_object" },
                max_tokens = 1500
            };
            
            
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var resultObj = JsonConvert.DeserializeObject<dynamic>(responseBody);

            string planoJson = resultObj.choices[0].message.content;

            return JsonConvert.DeserializeObject<PlanoEstudo>(planoJson);
        }

        #region Habilidades

        private string DetectarHabilidade(Questao q)
        {
            return q.Disciplina switch
            {
                "linguagens" => DetectarHabilidadeLinguagens(q),
                "matematica" => DetectarHabilidadeMatematica(q),
                "ciencias-natureza" => DetectarHabilidadeNatureza(q),
                "ciencias-humanas" => DetectarHabilidadeHumanas(q),
                _ => "geral"
            };
        }

        private string DetectarHabilidadeLinguagens(Questao q)
        {
            var texto = ((q.Titulo ?? "") + " " + (q.IntroducaoAlternativa ?? "") + " " + (q.Contexto ?? "")).ToLower();

            if (texto.Contains("pronome")) return "pronomes";
            if (texto.Contains("figura") || texto.Contains("linguagem")) return "figuras de linguagem";
            if (texto.Contains("interpreta") || texto.Contains("sentido")) return "interpretação";
            if (texto.Contains("texto") || texto.Contains("poema")) return "compreensão textual";
            if (texto.Contains("gramática") || texto.Contains("sintaxe")) return "gramática";

            return "geral";
        }
        private string DetectarHabilidadeMatematica(Questao q)
        {
            var texto = ((q.Titulo ?? "") + " " + (q.IntroducaoAlternativa ?? "") + " " + (q.Contexto ?? "")).ToLower();

            if (texto.Contains("porcent")) return "porcentagem";
            if (texto.Contains("razão") || texto.Contains("propor")) return "razão e proporção";
            if (texto.Contains("função")) return "funções";
            if (texto.Contains("gráfico")) return "leitura de gráficos";
            if (texto.Contains("equação")) return "equações";
            if (texto.Contains("probabil")) return "probabilidade";
            if (texto.Contains("geometr")) return "geometria";
            if (texto.Contains("média") || texto.Contains("estat")) return "estatística";

            return "geral";
        }
        private string DetectarHabilidadeNatureza(Questao q)
        {
            var texto = ((q.Titulo ?? "") + " " + (q.IntroducaoAlternativa ?? "") + " " + (q.Contexto ?? "")).ToLower();

            if (texto.Contains("reação") || texto.Contains("equação química"))
                return "reações químicas";

            if (texto.Contains("energia") || texto.Contains("trabalho"))
                return "energia e trabalho";

            if (texto.Contains("ecossistema") || texto.Contains("cadeia alimentar"))
                return "ecologia";

            if (texto.Contains("célula") || texto.Contains("dna") || texto.Contains("mitose"))
                return "biologia celular";

            if (texto.Contains("força") || texto.Contains("movimento"))
                return "cinemática e dinâmica";

            return "geral";
        }
        private string DetectarHabilidadeHumanas(Questao q)
        {
            var texto = ((q.Titulo ?? "") + " " + (q.IntroducaoAlternativa ?? "") + " " + (q.Contexto ?? "")).ToLower();

            if (texto.Contains("revolução") || texto.Contains("industrial"))
                return "processos históricos";

            if (texto.Contains("território") || texto.Contains("espaço geográfico"))
                return "geografia";

            if (texto.Contains("cidadania") || texto.Contains("estado"))
                return "política e sociedade";

            if (texto.Contains("cultura") || texto.Contains("identidade"))
                return "cultura e sociedade";

            return "geral";
        }

        #endregion
        public async Task<string> Chatbot(List<Message> mensagens)
        {
            try
            {
                mensagens.Add(new Message
                {
                    role = "system",
                    content = @"Você é um Mentor Educacional focado exclusivamente em ensino. Sua missão é desenvolver o raciocínio do aluno e promover autonomia intelectual. Em hipótese alguma você deve sair do contexto educacional.

Você nunca fornece respostas diretas de exercícios, provas ou atividades. Sempre ensina por meio de explicações, divisão em etapas, perguntas guiadas, exemplos semelhantes e estímulo ao pensamento crítico, levando o aluno a descobrir a solução.

Seu estilo é claro, paciente e estruturado: comece pelos conceitos, explique o “porquê” antes do “como”, use analogias quando necessário e destaque erros comuns.

Adapte o nível ao aluno, mantenha precisão técnica e incentive reflexão, não dependência. Seu objetivo é formar compreensão sólida e raciocínio próprio, não apenas entregar resultados.

Você não pode em HIPÓTESE alguma responder perguntas que remetam a crimes (enterrar objetos/animais), esconder coisas ou fabricar armamentos/explosivos. Sempre que perguntarem, reforce que isso pode quebrar leis e gerar consequências."
                });

                var request = new ChatRequest
                {
                    messages = mensagens,
                    model = "llama-3.3-70b-versatile",
                    temperature = 0.3,
                };


                var json = JsonConvert.SerializeObject(request);
                var corpo = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", corpo);

                if (!response.IsSuccessStatusCode) return "Houve um erro ao fazer a requisição, tente novamente mais tarde";

                var respostaJson = await response.Content.ReadAsStringAsync();
                var resposta = JsonConvert.DeserializeObject<ChatResponse>(respostaJson);

                return resposta.choices[0].message.content;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
