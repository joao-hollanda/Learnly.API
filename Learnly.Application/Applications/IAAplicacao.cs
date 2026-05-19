using System.Collections.ObjectModel;
using consumindoIA.Domain;
using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Learnly.Domain.Entities.Simulados;
using Learnly.Services.IAService;
using Newtonsoft.Json;

namespace Learnly.Application
{
    public class IAAplicacao : IIAAplicacao
    {
        private readonly IIAService _iaService;
        private readonly ISimuladoAplicacao _simuladoAplicacao;
        private readonly IPlanoAplicacao _planoAplicacao;

        public IAAplicacao(
            IIAService iaService,
            ISimuladoAplicacao simuladoAplicacao,
            IPlanoAplicacao planoAplicacao)
        {
            _iaService = iaService;
            _simuladoAplicacao = simuladoAplicacao;
            _planoAplicacao = planoAplicacao;
        }

        public async Task<string> GerarFeedbackAsync(int simuladoId, int usuarioId)
        {
            var simulado = await _simuladoAplicacao.Obter(simuladoId, usuarioId)
                ?? throw new KeyNotFoundException("Simulado não encontrado.");

            return await _iaService.GerarFeedbackAsync(simulado);
        }

        public async Task<PlanoEstudo> GerarPlanoAsync(int usuarioId, CriarPlanoIADTO dto)
        {
            dto.UsuarioId = usuarioId;
            dto.DataInicio = DateTime.UtcNow;
            dto.DataFim = DateTime.UtcNow.AddMonths(3);

            var planoBase = new PlanoEstudo
            {
                UsuarioId = usuarioId,
                Titulo = dto.Titulo,
                Objetivo = dto.Objetivo,
                HorasPorSemana = dto.HorasPorSemana,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim
            };

            var planoGerado = await _iaService.GerarPlanoAsync(planoBase);
            await _planoAplicacao.CriarDaIA(planoGerado);
            await _planoAplicacao.AtivarPlano(planoGerado.PlanoId, usuarioId);
            return planoGerado;
        }

        private const string SystemPromptChatbot = @"Você é um Mentor Educacional focado exclusivamente em ensino.
Sua missão é desenvolver o raciocínio do aluno e promover autonomia intelectual.
Em hipótese alguma você deve sair do contexto educacional.

Você nunca fornece respostas diretas de exercícios, provas ou atividades.
Sempre ensina por meio de explicações, divisão em etapas, perguntas guiadas,
exemplos semelhantes e estímulo ao pensamento crítico.

REGRAS DE FERRAMENTAS — siga obrigatoriamente:
- Se o aluno pedir para CRIAR ou GERAR um plano de estudos: colete APENAS título, objetivo e horasPorSemana, depois chame gerar_novo_plano_estudo imediatamente. NUNCA peça data de início, data de fim ou qualquer outro campo. NUNCA escreva o plano em texto.
- Se o aluno perguntar sobre seu desempenho ou notas: chame buscar_desempenho_do_aluno.
- Se o aluno perguntar sobre pontos fracos ou onde está errando: chame buscar_pontos_fracos_por_habilidade.
- Se o aluno quiser ver questões que errou: chame revisar_questoes_erradas.
- Se o aluno perguntar sobre seu plano atual: chame buscar_plano_estudo_atual.

PROIBIDO:
- Pedir data de início ou data de fim ao aluno em qualquer situação.
- Escrever um plano de estudos em texto — sempre use a ferramenta.
- Pedir confirmação antes de chamar uma ferramenta quando já tem os dados necessários.

Você não pode responder perguntas sobre crimes, esconder objetos ou fabricar
armamentos/explosivos. Reforce sempre que isso pode gerar consequências legais.";

        public async Task<object> ChatbotAsync(List<Message> mensagens, int usuarioId)
        {
            var system = mensagens.FirstOrDefault(m => m.role == "system");
            var historico = mensagens.Where(m => m.role != "system").TakeLast(10).ToList();

            var mensagensTruncadas = new List<Message>();
            if (system != null) mensagensTruncadas.Add(system);
            else mensagensTruncadas.Add(new Message { role = "system", content = SystemPromptChatbot });
            mensagensTruncadas.AddRange(historico);

            var resposta = await _iaService.EnviarMensagensAsync(new ChatRequest
            {
                messages = mensagensTruncadas,
                model = "qwen/qwen3-32b",
                temperature = 0.3,
                tools = ChatbotTools.ObterFerramentas(),
                tool_choice = "auto",
                parallel_tool_calls = true,
                max_tokens = null
            }) ?? throw new Exception("Erro ao contatar a IA.");

            mensagensTruncadas.Add(resposta);

            if (resposta.tool_calls?.Any() != true)
                return new { tipo = "texto", resposta = resposta.content };

            var toolFormulario = resposta.tool_calls.FirstOrDefault(t => t.function.name == "solicitar_formulario");
            if (toolFormulario != null)
            {
                var args = JsonConvert.DeserializeObject<dynamic>(toolFormulario.function.arguments);
                return new
                {
                    tipo = "formulario",
                    campos = args?.campos,
                    mensagem = args?.mensagem?.ToString() ?? "Preciso de algumas informações:"
                };
            }

            // Agrupa tools de alteração de disciplina para processar em batch
            var toolsAlteracao = resposta.tool_calls
                .Where(t => t.function.name == "adicionar_ou_remover_disciplina")
                .ToList();

            var toolsOutras = resposta.tool_calls
                .Where(t => t.function.name != "adicionar_ou_remover_disciplina")
                .ToList();

            // Processa alterações de disciplina em batch
            if (toolsAlteracao.Any())
            {
                var resultadoBatch = await AlterarDisciplinasBatch(usuarioId, toolsAlteracao.Select(t => t.function.arguments).ToList());
                foreach (var tool in toolsAlteracao)
                {
                    mensagensTruncadas.Add(new Message
                    {
                        role = "tool",
                        tool_call_id = tool.id,
                        name = tool.function.name,
                        content = resultadoBatch
                    });
                }
            }

            // Processa outras tools em paralelo normalmente
            if (toolsOutras.Any())
            {
                var tasks = toolsOutras.Select(async tool =>
                {
                    var resultado = await ExecutarFerramentaAsync(tool.function.name, tool.function.arguments, usuarioId);
                    return new Message
                    {
                        role = "tool",
                        tool_call_id = tool.id,
                        name = tool.function.name,
                        content = resultado
                    };
                });

                var resultados = await Task.WhenAll(tasks);
                mensagensTruncadas.AddRange(resultados);
            }

            var respostaFinal = await _iaService.EnviarMensagensAsync(new ChatRequest
            {
                messages = mensagensTruncadas,
                model = "qwen/qwen3-32b",
                temperature = 0.3,
                tools = null,
                tool_choice = null,
                parallel_tool_calls = null,
                max_tokens = null
            });

            return new { tipo = "texto", resposta = respostaFinal?.content ?? throw new Exception("Erro na resposta final da IA.") };
        }

        public async Task<List<ExplicacaoQuestao>> GerarExplicacoesAsync(int simuladoId, int usuarioId)
        {
            var simulado = await _simuladoAplicacao.Obter(simuladoId, usuarioId)
                ?? throw new KeyNotFoundException("Simulado não encontrado.");

            var questoesErradas = simulado.Questoes
                .Where(sq =>
                {
                    var resp = simulado.Respostas.FirstOrDefault(r => r.QuestaoId == sq.QuestaoId);
                    return resp?.Alternativa != null
                        && resp.Alternativa.Letra != sq.Questao?.AlternativaCorreta;
                })
                .ToList();

            var respostas = simulado.Respostas
                .Where(r => r.Alternativa != null)
                .ToDictionary(r => r.QuestaoId);

            return await _iaService.GerarExplicacoesAsync(questoesErradas, respostas);
        }

        private async Task<string> ExecutarFerramentaAsync(string nome, string args, int usuarioId)
        {
            try
            {
                return nome switch
                {
                    "buscar_desempenho_do_aluno" => await BuscarDesempenho(usuarioId),
                    "buscar_pontos_fracos_por_habilidade" => await BuscarPontosFracos(usuarioId, args),
                    "revisar_questoes_erradas" => await RevisarQuestoesErradas(usuarioId, args),
                    "buscar_plano_estudo_atual" => await BuscarPlanoAtual(usuarioId),
                    "gerar_novo_plano_estudo" => await GerarNovoPlano(usuarioId, args),
                    "reajustar_carga_horaria_plano" => await ReajustarCarga(usuarioId, args),
                    "reagendar_topicos_atrasados" => await ReagendarAtrasados(usuarioId),
                    _ => "Ferramenta não reconhecida."
                };
            }
            catch (Exception ex)
            {
                return $"Erro ao executar {nome}: {ex.Message}";
            }
        }

        private async Task<string> BuscarDesempenho(int usuarioId)
        {
            var simulados = await _simuladoAplicacao.Listar5(usuarioId);
            if (!simulados.Any()) return "O aluno não possui simulados concluídos.";

            var notas = simulados.Select(s => new
            {
                Data = s.Data.ToString("dd/MM/yyyy"),
                Nota = s.NotaFinal
            });

            return JsonConvert.SerializeObject(notas);
        }

        private async Task<string> BuscarPontosFracos(int usuarioId, string args)
        {
            var simList = await _simuladoAplicacao.Listar5(usuarioId);
            if (!simList.Any()) return "O aluno não tem simulados suficientes.";

            var dictHabilidades = new Dictionary<string, int>();

            foreach (var sim in simList)
                foreach (var sq in sim.Questoes ?? Enumerable.Empty<SimuladoQuestao>())
                {
                    if (sq.Questao == null) continue;
                    var resp = sim.Respostas?.FirstOrDefault(r => r.QuestaoId == sq.QuestaoId);
                    if (resp?.Alternativa == null || resp.Alternativa.Letra == sq.Questao.AlternativaCorreta) continue;

                    var habilidade = HabilidadeDetector.Detectar(sq.Questao);
                    dictHabilidades.TryAdd(habilidade, 0);
                    dictHabilidades[habilidade]++;
                }

            var top = dictHabilidades.OrderByDescending(x => x.Value).Take(3).Select(x => x.Key).ToList();
            return top.Any()
                ? "Habilidades mais fracas: " + string.Join(", ", top)
                : "Não há dados suficientes de erro ainda.";
        }

        private async Task<string> RevisarQuestoesErradas(int usuarioId, string args)
        {
            var sims = await _simuladoAplicacao.Listar5(usuarioId);
            var erradas = new List<object>();

            foreach (var s in sims)
            {
                if (s.Questoes == null || s.Respostas == null) continue;

                foreach (var sq in s.Questoes)
                {
                    var resp = s.Respostas.FirstOrDefault(r => r.QuestaoId == sq.QuestaoId);
                    if (resp?.Alternativa == null || sq.Questao == null) continue;
                    if (resp.Alternativa.Letra == sq.Questao.AlternativaCorreta) continue;

                    var correta = sq.Questao.Alternativas?.FirstOrDefault(a => a.Letra == sq.Questao.AlternativaCorreta);
                    erradas.Add(new
                    {
                        sq.Questao.Contexto,
                        sq.Questao.Titulo,
                        Marcada = resp.Alternativa.Texto,
                        Correta = correta?.Texto,
                        ExplicacaoExistente = resp.Explicacao
                    });

                    if (erradas.Count >= 2) break;
                }

                if (erradas.Count >= 2) break;
            }

            return erradas.Any()
                ? JsonConvert.SerializeObject(erradas)
                : "Nenhuma questão errada encontrada.";
        }

        private async Task<string> BuscarPlanoAtual(int usuarioId)
        {
            var plano = await _planoAplicacao.ObterPlanoAtivo(usuarioId);
            if (plano == null) return "Nenhum plano de estudo ativo no momento.";

            return JsonConvert.SerializeObject(new
            {
                plano.Titulo,
                plano.Objetivo,
                plano.HorasPorSemana,
                Materias = plano.PlanoMaterias?.Select(pm => new
                {
                    pm.Materia?.Nome,
                    pm.HorasTotais,
                    pm.HorasConcluidas
                })
            });
        }

        private async Task<string> GerarNovoPlano(int usuarioId, string args)
        {
            var raw = JsonConvert.DeserializeObject<Dictionary<string, object>>(args);
            if (raw == null) return "Erro: argumentos inválidos.";

            raw.TryGetValue("titulo", out var tituloObj);
            raw.TryGetValue("objetivo", out var objetivoObj);
            raw.TryGetValue("horasPorSemana", out var horasObj);

            var titulo = tituloObj?.ToString();
            var objetivo = objetivoObj?.ToString();
            int horas = 0;

            if (horasObj != null)
                int.TryParse(horasObj.ToString(), out horas);

            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(objetivo) || horas <= 0)
                return "Erro: título, objetivo e horasPorSemana são obrigatórios. Pergunte ao aluno o que está faltando.";

            var dto = new CriarPlanoIADTO
            {
                Titulo = titulo,
                Objetivo = objetivo,
                HorasPorSemana = horas,
                UsuarioId = usuarioId,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddMonths(3)
            };

            var plano = await GerarPlanoAsync(usuarioId, dto);
            return $"Plano '{plano.Titulo}' com {plano.HorasPorSemana}h/semana criado e ativado com sucesso.";
        }

        private async Task<string> ReajustarCarga(int usuarioId, string args)
        {
            var plano = await _planoAplicacao.ObterPlanoAtivo(usuarioId);
            if (plano == null) return "Nenhum plano ativo para reajustar.";

            var dict = JsonConvert.DeserializeObject<Dictionary<string, int>>(args);
            if (dict == null || !dict.TryGetValue("novaCargaHoraria", out int novaCarga))
                return "Erro: parâmetro novaCargaHoraria ausente.";

            plano.HorasPorSemana = novaCarga;
            await _planoAplicacao.Atualizar(plano);
            return $"Carga horária atualizada para {novaCarga}h/semana.";
        }

        private async Task<string> AlterarDisciplinasBatch(int usuarioId, List<string> argsList)
        {
            var plano = await _planoAplicacao.ObterPlanoAtivoComTracking(usuarioId);
            if (plano == null) return "Nenhum plano ativo.";

            var executadas = new List<string>();

            foreach (var args in argsList)
            {
                var dict = ParseArgs(args);
                if (dict == null) continue;

                dict.TryGetValue("acao", out var acao);
                dict.TryGetValue("disciplinaAlvo", out var alvo);
                dict.TryGetValue("disciplinaSubstituta", out var substituta);

                if (string.IsNullOrEmpty(acao) || string.IsNullOrEmpty(alvo)) continue;

                var existente = plano.PlanoMaterias?
                    .FirstOrDefault(m => m.Materia?.Nome?.Contains(alvo, StringComparison.OrdinalIgnoreCase) == true);

                if (acao == "remover")
                {
                    if (existente == null)
                    {
                        executadas.Add($"AVISO: '{alvo}' não existe no plano — nenhuma alteração feita");
                        continue;
                    }
                    plano.PlanoMaterias!.Remove(existente);
                    executadas.Add($"removida '{alvo}'");
                    continue;
                }
                if (acao == "substituir")
                {
                    if (existente != null)
                        plano.PlanoMaterias!.Remove(existente);

                    var nomeRaw = substituta ?? alvo;
                    var nome = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                        .ToTitleCase(nomeRaw?.ToLower() ?? "");

                    var topicos = await GerarTopicosParaDisciplina(nome, plano.Titulo);

                    plano.PlanoMaterias ??= new Collection<PlanoMateria>();
                    plano.PlanoMaterias.Add(new PlanoMateria
                    {
                        Materia = new Materia { Nome = nome, Cor = "#444444", GeradaPorIA = true },
                        HorasTotais = 5,
                        HorasConcluidas = 0,
                        Topicos = topicos
                    });

                    executadas.Add($"substituída '{alvo}' por '{nome}'");
                    continue;
                }

                if (acao == "adicionar")
                {
                    var nome = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                        .ToTitleCase(alvo?.ToLower() ?? "");

                    var topicos = await GerarTopicosParaDisciplina(nome, plano.Titulo);

                    plano.PlanoMaterias ??= new Collection<PlanoMateria>();
                    plano.PlanoMaterias.Add(new PlanoMateria
                    {
                        Materia = new Materia { Nome = nome, Cor = "#444444", GeradaPorIA = true },
                        HorasTotais = 5,
                        HorasConcluidas = 0,
                        Topicos = topicos
                    });

                    executadas.Add($"adicionada '{nome}'");
                    continue;
                }
            }

            await _planoAplicacao.Atualizar(plano);
            return executadas.Any()
                ? $"Ações executadas: {string.Join(", ", executadas)}."
                : "Nenhuma ação válida encontrada.";
        }
        private async Task<List<string>> GerarTopicosParaDisciplina(string nomeDisciplina, string tema)
        {
            try
            {
                var resposta = await _iaService.EnviarMensagensAsync(new ChatRequest
                {
                    model = "llama-3.1-8b-instant",
                    temperature = 0.3,
                    max_tokens = 300,
                    messages = new List<Message>
                    {
                        new() { role = "system", content = "Você gera listas de tópicos de estudo. Responda APENAS com um array JSON de strings, sem explicações, sem markdown, sem texto fora do JSON." },
                        new() { role = "user", content = $"Gere 6 tópicos de estudo essenciais para a disciplina \"{nomeDisciplina}\", do tema \"{tema}\". Retorne apenas o array JSON. Exemplo: [\"Tópico 1\", \"Tópico 2\"]" }
                    }
                });

                var raw = resposta?.content ?? "[]";

                var clean = raw.Trim()
                    .TrimStart("```json".ToCharArray())
                    .TrimStart("```".ToCharArray())
                    .TrimEnd("```".ToCharArray())
                    .Trim();

                return JsonConvert.DeserializeObject<List<string>>(clean)
                    ?? new List<string> { "Revisão Geral de " + nomeDisciplina };
            }
            catch
            {
                return new List<string> { "Revisão Geral de " + nomeDisciplina };
            }
        }

        private async Task<string> ReagendarAtrasados(int usuarioId)
        {
            var plano = await _planoAplicacao.ObterPlanoAtivo(usuarioId);
            if (plano == null) return "Nenhum plano ativo.";

            var atrasadas = plano.PlanoMaterias?
                .Where(pm => pm.HorasConcluidas < pm.HorasTotais)
                .Select(pm => pm.Materia?.Nome)
                .ToList();

            if (atrasadas?.Any() != true)
                return "O aluno está em dia, nenhum tópico atrasado.";

            plano.DataFim = plano.DataFim.AddDays(7);
            await _planoAplicacao.Atualizar(plano);
            return $"Plano estendido em 7 dias. Atenção: {string.Join(", ", atrasadas)}.";
        }

        private static Dictionary<string, string>? ParseArgs(string? args)
        {
            if (string.IsNullOrWhiteSpace(args)) return null;
            try { return JsonConvert.DeserializeObject<Dictionary<string, string>>(args); }
            catch { return null; }
        }
    }
}