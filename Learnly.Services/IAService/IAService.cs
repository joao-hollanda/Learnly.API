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
        private readonly IBuscaMaterialService _busca;

        public IAService(GroqHttpClient groq, IBuscaMaterialService busca)
        {
            _groq = groq;
            _busca = busca;
        }

        public async Task<string> GerarFeedbackAsync(Simulado simulado)
        {
            var resumoIA = GerarResumoIA(simulado);
            var jsonResumo = JsonConvert.SerializeObject(resumoIA);

            var habilidadesErros = simulado.Questoes
                .Where(sq =>
                {
                    var resp = simulado.Respostas.FirstOrDefault(r => r.QuestaoId == sq.QuestaoId);
                    return resp?.Alternativa != null && resp.Alternativa.Letra != sq.Questao?.AlternativaCorreta;
                })
                .GroupBy(sq => HabilidadeDetector.Detectar(sq.Questao))
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => $"{g.Key} ({g.Count()} erro(s))")
                .ToList();

            var habilidadesStr = habilidadesErros.Any()
                ? string.Join(", ", habilidadesErros)
                : "sem padrão claro";

            var acertos = simulado.Desempenho.QuantidadeDeAcertos;
            var total = simulado.Desempenho.QuantidadeDeQuestoes;
            var pct = total > 0 ? (double)acertos / total * 100 : 0;
            var nivel = pct >= 70 ? "avançado" : pct >= 45 ? "intermediário" : "iniciante";

            var mensagens = new List<Message>
            {
                new()
                {
                    role = "system",
                    content = @"Você é um professor experiente de cursinho pré-vestibular especializado no ENEM.
        Você conhece profundamente os materiais didáticos brasileiros e sabe exatamente quais recursos 
        recomendar para cada habilidade e nível de aluno.

        Suas respostas são diretas, específicas e acionáveis — nunca genéricas.
        Você menciona livros, canais, playlists e sites reais e conhecidos no Brasil.
        Você adapta a linguagem ao nível do aluno (iniciante, intermediário, avançado)."
                },
                new()
                {
                    role = "user",
                    content = $@"
                Resultado ENEM:
                - Acertos: {acertos}/{total}
                - Percentual: {pct:F0}%
                - Nível estimado: {nivel}

                Erros por habilidade:
                {habilidadesStr}

                Dados detalhados:
                {jsonResumo}

                Gere um diagnóstico curto, humano e preciso, baseado SOMENTE nos dados fornecidos.

                Regras importantes:
                - Não invente dificuldades sem evidência clara
                - Se houver poucos erros ou amostra pequena, diga explicitamente que ainda não há padrão consistente
                - Não afirmar 'nível avançado' com confiança alta se houver poucas questões
                - Não citar IDs internos, nomes técnicos, códigos ou identificadores de simulados
                - Evite frases genéricas, motivacionais ou de coach
                - O recurso recomendado deve ser realmente relevante para a principal dificuldade detectada
                - Se o desempenho for muito bom, foque em consistência, velocidade, aprofundamento ou manutenção de desempenho
                - Se não houver fraqueza clara, o recurso pode ser voltado para treino geral de ENEM
                - Máximo de 1 frase por seção
                - Linguagem direta e natural

                Responda EXATAMENTE neste formato:

                **Onde você está**
                [resumo objetivo do desempenho atual]

                **O que te trava**
                [principal gargalo identificado OU informe que ainda não existe padrão claro]

                **Recurso recomendado**
                [1 recurso específico e realmente relevante]

                **Próximo passo**
                [1 ação prática e concreta para as próximas 24h]"
                }
            };

            var request = new ChatRequest
            {
                model = "llama-3.3-70b-versatile",
                messages = mensagens,
                temperature = 0.4,
                max_tokens = 600
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
                    role = "system",
                    content = $@"- Materias devem ser relevantes para '{plano.Objetivo}'
            - O plano deve ter {totalMats} materias
            - Cada matéria deve ter 5-8 tópicos
            - JSON válido, apenas o objeto, sem explicações
            - O plano deve ter {horasTotais} horas totais distribuídas entre as matérias
            - Ordenado para suprir dependências"
                },
                new
                {
                    role = "user",
                    content = $@"Gere um plano de estudos '{plano.Titulo}' seguindo rigorosamente este schema JSON:
            {{
            ""HorasPorSemana"": {plano.HorasPorSemana},
            ""Ativo"": true,
            ""UsuarioId"": {plano.UsuarioId},
            ""PlanoMaterias"": [
                {{
                ""HorasTotais"": 10,
                ""HorasConcluidas"": 0,
                ""Topicos"": [""Tópico 1"", ""Tópico 2"", ""Tópico 3""],
                ""Materia"": {{
                    ""Nome"": ""Nome da Matéria"",
                    ""GeradaPorIA"": true,
                    ""Cor"": ""#4F46E5""
                }}
                }}
            ]
            }}"
                }
            };

            var requestBody = new
            {
                model = "llama-3.3-70b-versatile",
                messages,
                temperature = 0.2,
                response_format = new { type = "json_object" },
                max_tokens = 4000
            };

            var planoJson = await _groq.EnviarAsync(requestBody);

            if (planoJson == null)
                throw new Exception("Erro ao gerar plano IA: resposta nula.");

            var planoCriado = JsonConvert.DeserializeObject<PlanoEstudo>(planoJson);
            planoCriado.Titulo = plano.Titulo;
            planoCriado.UsuarioId = plano.UsuarioId;
            planoCriado.Objetivo = plano.Objetivo;
            planoCriado.DataInicio = plano.DataInicio;
            planoCriado.DataFim = plano.DataFim;

            return planoCriado ?? throw new Exception("Erro ao gerar plano IA: resposta inválida.");
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
                model = "llama-3.3-70b-versatile",
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

        public async Task<List<MaterialRecomendado>> GerarMateriaisAsync(Simulado simulado)
        {
            var resumoIA = GerarResumoIA(simulado);
            var jsonResumo = JsonConvert.SerializeObject(resumoIA);

            var habilidadesErros = simulado.Questoes
                .Where(sq =>
                {
                    var resp = simulado.Respostas.FirstOrDefault(r => r.QuestaoId == sq.QuestaoId);
                    return resp?.Alternativa != null && resp.Alternativa.Letra != sq.Questao?.AlternativaCorreta;
                })
                .GroupBy(sq => HabilidadeDetector.Detectar(sq.Questao))
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => $"{g.Key} ({g.Count()} erro(s))")
                .ToList();

            var habilidadesStr = habilidadesErros.Any()
                ? string.Join(", ", habilidadesErros)
                : "sem padrão claro";

            var mensagens = new List<Message>
            {
                new()
                {
                    role = "system",
                    content = @"Você é um professor especialista no ENEM que conhece os melhores canais e videoaulas gratuitas do YouTube no Brasil
(ex.: Ferretto, Professor Boaro, Toda Matéria, Descomplica, Stoodi, Equaciona).
Você recomenda APENAS videoaulas do YouTube, específicas e realmente úteis para a dificuldade do aluno. NUNCA inventa URLs."
                },
                new()
                {
                    role = "user",
                    content = $@"Com base no desempenho do aluno no ENEM, recomende videoaulas do YouTube.

                Erros por habilidade: {habilidadesStr}

                Dados detalhados:
                {jsonResumo}

                Regras:
                - Recomende de 3 a 4 videoaulas do YouTube
                - Priorize as áreas/habilidades com mais erros
                - Se não houver erros claros, recomende videoaulas de treino geral para o ENEM
                - 'termoBusca' deve ser um termo de busca específico em português que encontre essa videoaula no YouTube (inclua o nome do canal ou professor quando fizer sentido)
                - NÃO inclua URLs em nenhum campo
                - 'motivo' com no máximo 1 frase, dirigido ao aluno

                Responda EXATAMENTE neste formato JSON, sem textos fora do objeto:
                {{
                  ""materiais"": [
                    {{
                      ""titulo"": ""nome da videoaula ou canal"",
                      ""tipo"": ""Videoaula"",
                      ""area"": ""disciplina ou assunto"",
                      ""motivo"": ""por que ajuda no que o aluno errou"",
                      ""termoBusca"": ""termo de busca ideal no YouTube""
                    }}
                  ]
                }}"
                }
            };

            var requestBody = new
            {
                model = "llama-3.3-70b-versatile",
                messages = mensagens,
                temperature = 0.3,
                response_format = new { type = "json_object" },
                max_tokens = 1200
            };

            var raw = await _groq.EnviarAsync(requestBody);
            if (string.IsNullOrWhiteSpace(raw))
                return new List<MaterialRecomendado>();

            var wrapper = JsonConvert.DeserializeObject<MateriaisWrapper>(raw);
            var materiais = wrapper?.Materiais ?? new List<MaterialRecomendado>();

            await Task.WhenAll(materiais.Select(async material =>
            {
                material.Plataforma = "youtube";
                var termo = string.IsNullOrWhiteSpace(material.TermoBusca)
                    ? material.Titulo
                    : material.TermoBusca;
                material.Url = await _busca.ResolverUrlAsync(termo);
            }));

            return materiais;
        }

        private class MateriaisWrapper
        {
            public List<MaterialRecomendado> Materiais { get; set; }
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