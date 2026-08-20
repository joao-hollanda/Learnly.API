using consumindoIA.Domain;
using Learnly.Application;
using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Learnly.Domain.Entities.Simulados;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Learnly.Tests
{
    public class IAAplicacaoTests
    {
        private readonly Mock<IIAService> _iaService = new();
        private readonly Mock<ISimuladoAplicacao> _simuladoAplicacao = new();
        private readonly Mock<IPlanoAplicacao> _planoAplicacao = new();
        private readonly List<List<Message>> _chamadas = new();

        private IAAplicacao CriarSut() => new(_iaService.Object, _simuladoAplicacao.Object, _planoAplicacao.Object);

        private void ConfigurarRespostas(params Message[] respostas)
        {
            var fila = new Queue<Message>(respostas);

            _iaService
                .Setup(s => s.EnviarMensagensAsync(It.IsAny<ChatRequest>()))
                .Callback<ChatRequest>(r => _chamadas.Add(r.messages.ToList()))
                .ReturnsAsync(() => fila.Count > 0 ? fila.Dequeue() : null);
        }

        private static Message Texto(string conteudo) => new() { role = "assistant", content = conteudo };

        private static Message ComFerramenta(string nome, string argumentos = "{}") => new()
        {
            role = "assistant",
            tool_calls = new List<ToolCall>
            {
                new() { id = "call_1", type = "function", function = new FunctionCall { name = nome, arguments = argumentos } }
            }
        };

        private string ConteudoDaFerramenta() =>
            _chamadas.Last().Last(m => m.role == "tool").content;

        private static string Campo(object retorno, string nome) =>
            retorno.GetType().GetProperty(nome)?.GetValue(retorno)?.ToString();

        private static Simulado SimuladoComErro(string disciplina, string titulo) => new()
        {
            SimuladoId = 1,
            Data = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            NotaFinal = 700,
            Questoes = new List<SimuladoQuestao>
            {
                new()
                {
                    QuestaoId = 10,
                    Questao = new Questao
                    {
                        QuestaoId = 10,
                        Disciplina = disciplina,
                        Titulo = titulo,
                        Contexto = "contexto",
                        AlternativaCorreta = "A",
                        Alternativas = new List<Alternativa>
                        {
                            new() { Letra = "A", Texto = "certa" },
                            new() { Letra = "B", Texto = "errada" }
                        }
                    }
                }
            },
            Respostas = new List<RespostaSimulado>
            {
                new() { QuestaoId = 10, Alternativa = new Alternativa { Letra = "B", Texto = "errada" } }
            }
        };

        [Fact]
        public async Task GerarFeedbackAsync_SimuladoInexistente_LancaKeyNotFound()
        {
            _simuladoAplicacao.Setup(a => a.Obter(1, 7)).ReturnsAsync((Simulado)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GerarFeedbackAsync(1, 7));
        }

        [Fact]
        public async Task GerarFeedbackAsync_SimuladoExistente_DelegaParaIA()
        {
            var simulado = new Simulado { SimuladoId = 1 };
            _simuladoAplicacao.Setup(a => a.Obter(1, 7)).ReturnsAsync(simulado);
            _iaService.Setup(s => s.GerarFeedbackAsync(simulado)).ReturnsAsync("bom desempenho");
            var sut = CriarSut();

            Assert.Equal("bom desempenho", await sut.GerarFeedbackAsync(1, 7));
        }

        [Fact]
        public async Task GerarPlanoAsync_SemDatas_AssumeTresMesesEAtivaOPlano()
        {
            PlanoEstudo enviado = null;
            _iaService.Setup(s => s.GerarPlanoAsync(It.IsAny<PlanoEstudo>()))
                .Callback<PlanoEstudo>(p => enviado = p)
                .ReturnsAsync(() => new PlanoEstudo { PlanoId = 3, Titulo = enviado.Titulo });
            var sut = CriarSut();

            var dto = new CriarPlanoIADTO { Titulo = "Plano IA", Objetivo = "ENEM", HorasPorSemana = 10 };
            var plano = await sut.GerarPlanoAsync(7, dto);

            Assert.Equal(7, dto.UsuarioId);
            Assert.Equal(DateTimeKind.Utc, dto.DataInicio.Kind);
            Assert.Equal(dto.DataInicio.AddMonths(3), dto.DataFim);
            Assert.Equal(7, enviado.UsuarioId);
            Assert.Equal(10, enviado.HorasPorSemana);
            _planoAplicacao.Verify(a => a.CriarDaIA(plano), Times.Once);
            _planoAplicacao.Verify(a => a.AtivarPlano(3, 7), Times.Once);
        }

        [Fact]
        public async Task GerarPlanoAsync_ComDatas_PreservaPeriodoInformado()
        {
            var inicio = new DateTime(2026, 2, 1);
            var fim = new DateTime(2026, 6, 1);
            _iaService.Setup(s => s.GerarPlanoAsync(It.IsAny<PlanoEstudo>()))
                .ReturnsAsync(new PlanoEstudo { PlanoId = 3 });
            var sut = CriarSut();

            var dto = new CriarPlanoIADTO
            {
                Titulo = "Plano IA",
                Objetivo = "ENEM",
                HorasPorSemana = 10,
                DataInicio = inicio,
                DataFim = fim
            };
            await sut.GerarPlanoAsync(7, dto);

            Assert.Equal(inicio, dto.DataInicio);
            Assert.Equal(fim, dto.DataFim);
            Assert.Equal(DateTimeKind.Utc, dto.DataFim.Kind);
        }

        [Fact]
        public async Task ChatbotAsync_IaIndisponivel_LancaExcecao()
        {
            ConfigurarRespostas();
            var sut = CriarSut();

            await Assert.ThrowsAsync<Exception>(
                () => sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "oi" } }, 7));
        }

        [Fact]
        public async Task ChatbotAsync_SemFerramentas_RetornaTexto()
        {
            ConfigurarRespostas(Texto("olá, como posso ajudar?"));
            var sut = CriarSut();

            var retorno = await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "oi" } }, 7);

            Assert.Equal("texto", Campo(retorno, "tipo"));
            Assert.Equal("olá, como posso ajudar?", Campo(retorno, "resposta"));
        }

        [Fact]
        public async Task ChatbotAsync_SemSystemPrompt_InsereOPadraoETruncaHistorico()
        {
            ConfigurarRespostas(Texto("ok"));
            var mensagens = Enumerable.Range(1, 15)
                .Select(i => new Message { role = "user", content = $"mensagem {i}" })
                .ToList();
            var sut = CriarSut();

            await sut.ChatbotAsync(mensagens, 7);

            var enviadas = _chamadas.Single();
            Assert.Equal(11, enviadas.Count);
            Assert.Equal("system", enviadas[0].role);
            Assert.Contains("Mentor Educacional", enviadas[0].content);
            Assert.Equal("mensagem 6", enviadas[1].content);
        }

        [Fact]
        public async Task ChatbotAsync_ComSystemPrompt_MantemODoChamador()
        {
            ConfigurarRespostas(Texto("ok"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message>
            {
                new() { role = "system", content = "prompt customizado" },
                new() { role = "user", content = "oi" }
            }, 7);

            Assert.Equal("prompt customizado", _chamadas.Single()[0].content);
        }

        [Fact]
        public async Task ChatbotAsync_FerramentaDeFormulario_RetornaCamposSolicitados()
        {
            ConfigurarRespostas(ComFerramenta("solicitar_formulario",
                "{\"campos\":[{\"nome\":\"titulo\"}],\"mensagem\":\"Preciso de alguns dados\"}"));
            var sut = CriarSut();

            var retorno = await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "criar plano" } }, 7);

            Assert.Equal("formulario", Campo(retorno, "tipo"));
            Assert.Equal("Preciso de alguns dados", Campo(retorno, "mensagem"));
            Assert.NotNull(retorno.GetType().GetProperty("campos").GetValue(retorno));
        }

        [Fact]
        public async Task ChatbotAsync_BuscarDesempenho_SemSimulados_InformaAusencia()
        {
            _simuladoAplicacao.Setup(a => a.Listar(7, It.IsAny<int>())).ReturnsAsync(new List<Simulado>());
            ConfigurarRespostas(ComFerramenta("buscar_desempenho_do_aluno"), Texto("resposta final"));
            var sut = CriarSut();

            var retorno = await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "como vou?" } }, 7);

            Assert.Equal("O aluno não possui simulados concluídos.", ConteudoDaFerramenta());
            Assert.Equal("resposta final", Campo(retorno, "resposta"));
        }

        [Fact]
        public async Task ChatbotAsync_BuscarDesempenho_ComSimulados_SerializaNotas()
        {
            _simuladoAplicacao.Setup(a => a.Listar(7, It.IsAny<int>()))
                .ReturnsAsync(new List<Simulado> { SimuladoComErro("matematica", "Calcule a área") });
            ConfigurarRespostas(ComFerramenta("buscar_desempenho_do_aluno"), Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "como vou?" } }, 7);

            Assert.Contains("01/05/2026", ConteudoDaFerramenta());
            Assert.Contains("700", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_BuscarPontosFracos_AgrupaPorHabilidade()
        {
            _simuladoAplicacao.Setup(a => a.Listar(7, It.IsAny<int>()))
                .ReturnsAsync(new List<Simulado> { SimuladoComErro("matematica", "Calcule a área do triângulo") });
            ConfigurarRespostas(ComFerramenta("buscar_pontos_fracos_por_habilidade"), Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "onde erro mais?" } }, 7);

            Assert.Equal("Habilidades mais fracas: geometria", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_BuscarPontosFracos_SemSimulados_InformaAusencia()
        {
            _simuladoAplicacao.Setup(a => a.Listar(7, It.IsAny<int>())).ReturnsAsync(new List<Simulado>());
            ConfigurarRespostas(ComFerramenta("buscar_pontos_fracos_por_habilidade"), Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "onde erro mais?" } }, 7);

            Assert.Equal("O aluno não tem simulados suficientes.", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_BuscarPontosFracos_SemErros_InformaFaltaDeDados()
        {
            var simulado = SimuladoComErro("matematica", "Calcule a área do triângulo");
            simulado.Respostas[0].Alternativa.Letra = "A";
            _simuladoAplicacao.Setup(a => a.Listar(7, It.IsAny<int>()))
                .ReturnsAsync(new List<Simulado> { simulado });
            ConfigurarRespostas(ComFerramenta("buscar_pontos_fracos_por_habilidade"), Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "onde erro mais?" } }, 7);

            Assert.Equal("Não há dados suficientes de erro ainda.", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_RevisarQuestoesErradas_RetornaQuestaoComGabarito()
        {
            _simuladoAplicacao.Setup(a => a.Listar(7, It.IsAny<int>()))
                .ReturnsAsync(new List<Simulado> { SimuladoComErro("matematica", "Calcule a área") });
            ConfigurarRespostas(ComFerramenta("revisar_questoes_erradas"), Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "o que errei?" } }, 7);

            var conteudo = ConteudoDaFerramenta();
            Assert.Contains("Calcule a área", conteudo);
            Assert.Contains("certa", conteudo);
            Assert.Contains("errada", conteudo);
        }

        [Fact]
        public async Task ChatbotAsync_RevisarQuestoesErradas_SemErros_InformaAusencia()
        {
            var simulado = SimuladoComErro("matematica", "Calcule a área");
            simulado.Respostas[0].Alternativa.Letra = "A";
            _simuladoAplicacao.Setup(a => a.Listar(7, It.IsAny<int>()))
                .ReturnsAsync(new List<Simulado> { simulado });
            ConfigurarRespostas(ComFerramenta("revisar_questoes_erradas"), Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "o que errei?" } }, 7);

            Assert.Equal("Nenhuma questão errada encontrada.", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_BuscarPlanoAtual_SemPlano_InformaAusencia()
        {
            _planoAplicacao.Setup(a => a.ObterPlanoAtivo(7)).ReturnsAsync((PlanoEstudo)null);
            ConfigurarRespostas(ComFerramenta("buscar_plano_estudo_atual"), Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "meu plano?" } }, 7);

            Assert.Equal("Nenhum plano de estudo ativo no momento.", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_BuscarPlanoAtual_ComPlano_SerializaMaterias()
        {
            _planoAplicacao.Setup(a => a.ObterPlanoAtivo(7)).ReturnsAsync(new PlanoEstudo
            {
                Titulo = "Plano ENEM",
                Objetivo = "Medicina",
                HorasPorSemana = 12,
                PlanoMaterias = new List<PlanoMateria>
                {
                    new()
                    {
                        Materia = new Materia { Nome = "Biologia" },
                        HorasTotais = 10,
                        HorasConcluidas = 4
                    }
                }
            });
            ConfigurarRespostas(ComFerramenta("buscar_plano_estudo_atual"), Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "meu plano?" } }, 7);

            var conteudo = ConteudoDaFerramenta();
            Assert.Contains("Plano ENEM", conteudo);
            Assert.Contains("Biologia", conteudo);
        }

        [Fact]
        public async Task ChatbotAsync_ReajustarCarga_AtualizaHorasDoPlano()
        {
            var plano = new PlanoEstudo { PlanoId = 3, HorasPorSemana = 10 };
            _planoAplicacao.Setup(a => a.ObterPlanoAtivo(7)).ReturnsAsync(plano);
            ConfigurarRespostas(
                ComFerramenta("reajustar_carga_horaria_plano", "{\"novaCargaHoraria\":18}"),
                Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "quero estudar mais" } }, 7);

            Assert.Equal(18, plano.HorasPorSemana);
            Assert.Equal("Carga horária atualizada para 18h/semana.", ConteudoDaFerramenta());
            _planoAplicacao.Verify(a => a.Atualizar(plano), Times.Once);
        }

        [Fact]
        public async Task ChatbotAsync_ReajustarCarga_ArgumentoAusente_RetornaErro()
        {
            _planoAplicacao.Setup(a => a.ObterPlanoAtivo(7)).ReturnsAsync(new PlanoEstudo { PlanoId = 3 });
            ConfigurarRespostas(
                ComFerramenta("reajustar_carga_horaria_plano", "{\"outro\":18}"),
                Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "quero estudar mais" } }, 7);

            Assert.Equal("Erro: parâmetro novaCargaHoraria ausente.", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_ReajustarCarga_SemPlanoAtivo_InformaAusencia()
        {
            _planoAplicacao.Setup(a => a.ObterPlanoAtivo(7)).ReturnsAsync((PlanoEstudo)null);
            ConfigurarRespostas(
                ComFerramenta("reajustar_carga_horaria_plano", "{\"novaCargaHoraria\":18}"),
                Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "quero estudar mais" } }, 7);

            Assert.Equal("Nenhum plano ativo para reajustar.", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_ReagendarAtrasados_EstendePlanoEmSeteDias()
        {
            var dataFim = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var plano = new PlanoEstudo
            {
                PlanoId = 3,
                DataFim = dataFim,
                PlanoMaterias = new List<PlanoMateria>
                {
                    new() { Materia = new Materia { Nome = "Química" }, HorasTotais = 10, HorasConcluidas = 2 }
                }
            };
            _planoAplicacao.Setup(a => a.ObterPlanoAtivo(7)).ReturnsAsync(plano);
            ConfigurarRespostas(ComFerramenta("reagendar_topicos_atrasados"), Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "estou atrasado" } }, 7);

            Assert.Equal(dataFim.AddDays(7), plano.DataFim);
            Assert.Contains("Química", ConteudoDaFerramenta());
            _planoAplicacao.Verify(a => a.Atualizar(plano), Times.Once);
        }

        [Fact]
        public async Task ChatbotAsync_ReagendarAtrasados_SemAtraso_NaoAlteraPlano()
        {
            var dataFim = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var plano = new PlanoEstudo
            {
                PlanoId = 3,
                DataFim = dataFim,
                PlanoMaterias = new List<PlanoMateria>
                {
                    new() { Materia = new Materia { Nome = "Química" }, HorasTotais = 10, HorasConcluidas = 10 }
                }
            };
            _planoAplicacao.Setup(a => a.ObterPlanoAtivo(7)).ReturnsAsync(plano);
            ConfigurarRespostas(ComFerramenta("reagendar_topicos_atrasados"), Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "estou atrasado?" } }, 7);

            Assert.Equal(dataFim, plano.DataFim);
            Assert.Equal("O aluno está em dia, nenhum tópico atrasado.", ConteudoDaFerramenta());
            _planoAplicacao.Verify(a => a.Atualizar(It.IsAny<PlanoEstudo>()), Times.Never);
        }

        [Fact]
        public async Task ChatbotAsync_FerramentaDesconhecida_InformaNaoReconhecida()
        {
            ConfigurarRespostas(ComFerramenta("ferramenta_inexistente"), Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "?" } }, 7);

            Assert.Equal("Ferramenta não reconhecida.", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_FerramentaComFalha_RetornaMensagemDeErro()
        {
            _planoAplicacao.Setup(a => a.ObterPlanoAtivo(7)).ThrowsAsync(new InvalidOperationException("banco fora"));
            ConfigurarRespostas(ComFerramenta("buscar_plano_estudo_atual"), Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "meu plano?" } }, 7);

            Assert.Equal("Erro ao executar buscar_plano_estudo_atual: banco fora", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_RespostaFinalVazia_LancaExcecao()
        {
            _planoAplicacao.Setup(a => a.ObterPlanoAtivo(7)).ReturnsAsync((PlanoEstudo)null);
            ConfigurarRespostas(ComFerramenta("buscar_plano_estudo_atual"));
            var sut = CriarSut();

            await Assert.ThrowsAsync<Exception>(
                () => sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "meu plano?" } }, 7));
        }

        [Fact]
        public async Task ChatbotAsync_AdicionarDisciplina_IncluiMateriaComTopicosDaIA()
        {
            var plano = new PlanoEstudo { PlanoId = 3, Titulo = "Plano ENEM", PlanoMaterias = new List<PlanoMateria>() };
            _planoAplicacao.Setup(a => a.ObterPlanoAtivoComTracking(7)).ReturnsAsync(plano);
            ConfigurarRespostas(
                ComFerramenta("adicionar_ou_remover_disciplina", "{\"acao\":\"adicionar\",\"disciplinaAlvo\":\"biologia\"}"),
                Texto("[\"Citologia\", \"Genética\"]"),
                Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "adiciona biologia" } }, 7);

            var adicionada = Assert.Single(plano.PlanoMaterias);
            Assert.Equal("Biologia", adicionada.Materia.Nome);
            Assert.True(adicionada.Materia.GeradaPorIA);
            Assert.Equal(5, adicionada.HorasTotais);
            Assert.Equal(new[] { "Citologia", "Genética" }, adicionada.Topicos);
            Assert.Contains("adicionada 'Biologia'", ConteudoDaFerramenta());
            _planoAplicacao.Verify(a => a.Atualizar(plano), Times.Once);
        }

        [Fact]
        public async Task ChatbotAsync_AdicionarDisciplina_IaSemTopicosValidos_UsaRevisaoGeral()
        {
            var plano = new PlanoEstudo { PlanoId = 3, Titulo = "Plano ENEM", PlanoMaterias = new List<PlanoMateria>() };
            _planoAplicacao.Setup(a => a.ObterPlanoAtivoComTracking(7)).ReturnsAsync(plano);
            ConfigurarRespostas(
                ComFerramenta("adicionar_ou_remover_disciplina", "{\"acao\":\"adicionar\",\"disciplinaAlvo\":\"biologia\"}"),
                Texto("não consegui gerar"),
                Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "adiciona biologia" } }, 7);

            Assert.Equal(new[] { "Revisão Geral de Biologia" }, Assert.Single(plano.PlanoMaterias).Topicos);
        }

        [Fact]
        public async Task ChatbotAsync_RemoverDisciplina_TiraMateriaDoPlano()
        {
            var plano = new PlanoEstudo
            {
                PlanoId = 3,
                Titulo = "Plano ENEM",
                PlanoMaterias = new List<PlanoMateria>
                {
                    new() { Materia = new Materia { Nome = "Biologia" } }
                }
            };
            _planoAplicacao.Setup(a => a.ObterPlanoAtivoComTracking(7)).ReturnsAsync(plano);
            ConfigurarRespostas(
                ComFerramenta("adicionar_ou_remover_disciplina", "{\"acao\":\"remover\",\"disciplinaAlvo\":\"biologia\"}"),
                Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "tira biologia" } }, 7);

            Assert.Empty(plano.PlanoMaterias);
            Assert.Contains("removida 'biologia'", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_RemoverDisciplinaAusente_RetornaAviso()
        {
            var plano = new PlanoEstudo { PlanoId = 3, Titulo = "Plano ENEM", PlanoMaterias = new List<PlanoMateria>() };
            _planoAplicacao.Setup(a => a.ObterPlanoAtivoComTracking(7)).ReturnsAsync(plano);
            ConfigurarRespostas(
                ComFerramenta("adicionar_ou_remover_disciplina", "{\"acao\":\"remover\",\"disciplinaAlvo\":\"biologia\"}"),
                Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "tira biologia" } }, 7);

            Assert.Contains("AVISO", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_SubstituirDisciplina_TrocaMateriaDoPlano()
        {
            var plano = new PlanoEstudo
            {
                PlanoId = 3,
                Titulo = "Plano ENEM",
                PlanoMaterias = new List<PlanoMateria>
                {
                    new() { Materia = new Materia { Nome = "Biologia" } }
                }
            };
            _planoAplicacao.Setup(a => a.ObterPlanoAtivoComTracking(7)).ReturnsAsync(plano);
            ConfigurarRespostas(
                ComFerramenta("adicionar_ou_remover_disciplina",
                    "{\"acao\":\"substituir\",\"disciplinaAlvo\":\"biologia\",\"disciplinaSubstituta\":\"química\"}"),
                Texto("[\"Estequiometria\"]"),
                Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "troca biologia por química" } }, 7);

            var materia = Assert.Single(plano.PlanoMaterias);
            Assert.Equal("Química", materia.Materia.Nome);
            Assert.Contains("substituída 'biologia' por 'Química'", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_AlterarDisciplina_SemPlanoAtivo_InformaAusencia()
        {
            _planoAplicacao.Setup(a => a.ObterPlanoAtivoComTracking(7)).ReturnsAsync((PlanoEstudo)null);
            ConfigurarRespostas(
                ComFerramenta("adicionar_ou_remover_disciplina", "{\"acao\":\"adicionar\",\"disciplinaAlvo\":\"biologia\"}"),
                Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "adiciona biologia" } }, 7);

            Assert.Equal("Nenhum plano ativo.", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task ChatbotAsync_AlterarDisciplina_ArgumentosIncompletos_NaoExecutaAcao()
        {
            var plano = new PlanoEstudo { PlanoId = 3, Titulo = "Plano ENEM", PlanoMaterias = new List<PlanoMateria>() };
            _planoAplicacao.Setup(a => a.ObterPlanoAtivoComTracking(7)).ReturnsAsync(plano);
            ConfigurarRespostas(
                ComFerramenta("adicionar_ou_remover_disciplina", "{\"acao\":\"adicionar\"}"),
                Texto("resposta final"));
            var sut = CriarSut();

            await sut.ChatbotAsync(new List<Message> { new() { role = "user", content = "adiciona" } }, 7);

            Assert.Empty(plano.PlanoMaterias);
            Assert.Equal("Nenhuma ação válida encontrada.", ConteudoDaFerramenta());
        }

        [Fact]
        public async Task GerarExplicacoesAsync_SimuladoInexistente_LancaKeyNotFound()
        {
            _simuladoAplicacao.Setup(a => a.Obter(1, 7)).ReturnsAsync((Simulado)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GerarExplicacoesAsync(1, 7));
        }

        [Fact]
        public async Task GerarExplicacoesAsync_EnviaSomenteQuestoesErradas()
        {
            var simulado = SimuladoComErro("matematica", "Calcule a área");
            simulado.Questoes.Add(new SimuladoQuestao
            {
                QuestaoId = 11,
                Questao = new Questao { QuestaoId = 11, AlternativaCorreta = "C" }
            });
            simulado.Respostas.Add(new RespostaSimulado
            {
                QuestaoId = 11,
                Alternativa = new Alternativa { Letra = "C" }
            });
            _simuladoAplicacao.Setup(a => a.Obter(1, 7)).ReturnsAsync(simulado);
            var explicacoes = new List<ExplicacaoQuestao> { new() };
            _simuladoAplicacao
                .Setup(a => a.ObterOuGerarExplicacoes(It.Is<List<SimuladoQuestao>>(q =>
                    q.Count == 1 && q[0].QuestaoId == 10)))
                .ReturnsAsync(explicacoes);
            var sut = CriarSut();

            Assert.Same(explicacoes, await sut.GerarExplicacoesAsync(1, 7));
        }
    }
}
