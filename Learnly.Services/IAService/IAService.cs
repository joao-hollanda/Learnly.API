using consumindoIA.Domain;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Simulados;
using Newtonsoft.Json;

namespace Learnly.Services.IAService
{
    public class IAService : IIAService
    {
        private readonly GroqHttpClient _groq;

        public IAService(GroqHttpClient groq)
        {
            _groq = groq;
        }

        public async Task<string> GerarFeedbackAsync(Simulado simulado)
        {
            var resumoIA = GerarResumoIA(simulado);
            var jsonResumo = JsonConvert.SerializeObject(resumoIA, Formatting.Indented);

            var mensagens = new List<Message>
            {
                new()
                {
                    role    = "system",
                    content = @"Você é um mentor direto e experiente no ENEM. 
Você analisa dados de simulado e entrega um diagnóstico honesto em linguagem simples — 
sem enrolação, sem elogios vazios. Fale como alguém que já corrigiu milhares de provas."
                },
                new()
                {
                    role    = "user",
                    content = $@"Analise os dados deste simulado e gere um feedback em 3 blocos curtos (máximo 3 linhas cada):
{simulado.Desempenho.QuantidadeDeAcertos}/{simulado.Desempenho.QuantidadeDeQuestoes} acertos

{jsonResumo}

**Onde você está:** diagnóstico do nível atual — o aluno está errando por falta de base ou por gestão de prova?

**O que te trava:** qual é o padrão de erro? Conecte áreas, tempo gasto e tipo de questão em uma frase cirúrgica.

**O que fazer amanhã:** uma ação concreta e específica — conteúdo ou estratégia, não generalidades.

Seja direto. Use **negrito** só para o que realmente importa. Sem listas, sem introdução, sem conclusão motivacional."
                }
            };

            var request = new ChatRequest
            {
                model = "llama-3.1-8b-instant",
                messages = mensagens,
                temperature = 0.5
            };

            return await _groq.EnviarAsync(request) ?? "Erro ao gerar feedback.";
        }

        public async Task<PlanoEstudo> GerarPlanoAsync(PlanoEstudo plano)
        {
            int dias = (int)(plano.DataFim - plano.DataInicio).TotalDays;
            double semanas = dias / 7.0;
            int horasTotais = (int)Math.Ceiling(semanas * plano.HorasPorSemana);
            int totalMats = Math.Clamp((int)Math.Ceiling(semanas / 2.0), 4, 8);

            var messages = new[]
            {
                new
                {
                    role    = "system",
                    content = $@"- Materias devem ser relevantes para '{plano.Objetivo}'
- O plano deve ter {totalMats} materias
- Cada matéria 5-8 tópicos
- Datas 2026
- JSON válido, apenas o objeto, sem explicações
- O plano deve ter {horasTotais} horas totais
- Deve estar ordenado para suprir dependências"
                },
                new
                {
                    role    = "user",
                    content = $@"Gere um plano de estudos '{plano.Titulo}' seguindo rigorosamente este schema:
{{
  ""HorasPorSemana"": {plano.HorasPorSemana},
  ""Ativo"": true,
  ""UsuarioId"": {plano.UsuarioId},
  ""PlanoMaterias"": [
    {{
      ""Materia"": {{
        ""Nome"": ""Nome da Matéria"",
        ""GeradaPorIA"": true,
        ""Cor"": ""Cor em formato HEX (#RRGGBB)"",
        ""HorasTotais"": ""calcule conforme a prioridade e as horas totais"",
        ""Topicos"": [""Tópico 1"", ""Tópico 2""]
      }}
    }}
  ]
}}"
                }
            };

            var requestBody = new
            {
                model = "openai/gpt-oss-120b",
                messages,
                temperature = 0.2,
                response_format = new { type = "json_object" },
                max_tokens = 4000
            };

            var planoJson = await _groq.EnviarAsync(requestBody)
                ?? throw new Exception("Erro ao gerar plano IA: resposta nula.");

            return JsonConvert.DeserializeObject<PlanoEstudo>(planoJson)
                ?? throw new Exception("Resposta da IA não pôde ser desserializada em PlanoEstudo.");
        }

        public async Task<Message?> EnviarMensagensAsync(ChatRequest request)
        {
            return await _groq.EnviarCompletaAsync(request);
        }

        public async Task<List<ExplicacaoQuestao>> GerarExplicacoesAsync(
            List<SimuladoQuestao> questoesErradas,
            Dictionary<int, RespostaSimulado> respostas)
        {
            var questoesJson = JsonConvert.SerializeObject(
                questoesErradas.Select(q =>
                {
                    var respostaUsuario = respostas.GetValueOrDefault(q.QuestaoId);
                    return new
                    {
                        q.QuestaoId,
                        q.Questao.Titulo,
                        q.Questao.Contexto,
                        Alternativas = q.Questao.Alternativas.Select(a => new
                        {
                            a.AlternativaId,
                            a.Texto,
                            a.Correta
                        }),
                        RespostaUsuario = new
                        {
                            respostaUsuario?.AlternativaId,
                            respostaUsuario?.Alternativa?.Texto
                        },
                        RespostaCorreta = q.Questao.Alternativas
                            .Where(a => a.Correta)
                            .Select(a => new { a.AlternativaId, a.Texto })
                            .FirstOrDefault()
                    };
                }),
                Formatting.Indented
            );

            var mensagens = new List<Message>
            {
                new()
                {
                    role    = "system",
                    content = $@"Para cada questão em que o aluno errou no simulado do ENEM, responda: por que está errada?

Liste cada questão recebida e explique de forma clara, direta e didática o motivo do erro, destacando por que a alternativa escolhida não está correta e qual seria o raciocínio adequado para chegar à resposta certa. Dirija o feedback diretamente ao aluno, usando VOCÊ como pronome.

{questoesJson}

Responda usando EXCLUSIVAMENTE um array JSON no seguinte formato (sem comentários, sem textos fora do JSON, explicacao em markdown):

[
  {{
    ""QuestaoId"": <id da questão>,
    ""Explicacao"": ""explique por que a resposta do aluno está errada e como chegar à correta, em linguagem acessível""
  }}
]

Não adicione quaisquer textos fora desse objeto JSON."
                }
            };

            var requestBody = new
            {
                model = "llama-3.1-8b-instant",
                messages = mensagens,
                temperature = 0.2,
            };

            var raw = await _groq.EnviarAsync(requestBody)
                ?? throw new Exception("Erro ao gerar explicações: resposta nula.");

            var json = raw
                .Trim()
                .TrimStart("```json".ToCharArray())
                .TrimStart("```".ToCharArray())
                .TrimEnd("```".ToCharArray())
                .Trim();

            return JsonConvert.DeserializeObject<List<ExplicacaoQuestao>>(json)
                ?? throw new Exception("Não foi possível desserializar as explicações.");
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
                var habilidade = HabilidadeDetector.Detectar(questao);

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

                if (resposta.Alternativa.Letra == questao.AlternativaCorreta)
                {
                    bloco.Acertos++;
                }
                else
                {
                    bloco.ErrosPorHabilidade.TryAdd(habilidade, 0);
                    bloco.ErrosPorHabilidade[habilidade]++;
                }
            }

            return resumo;
        }
    }
}