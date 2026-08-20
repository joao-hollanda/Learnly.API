using consumindoIA.Domain;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Simulados;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Learnly.Tests
{
    public class IAServiceTests
    {
        private readonly ManipuladorHttpFalso _http = new();
        private readonly Mock<IBuscaMaterialService> _busca = new();

        private Learnly.Services.IAService.IAService CriarSut() =>
            new(new Learnly.Services.IAService.GroqHttpClient(_http.CriarCliente()), _busca.Object);

        private static Simulado SimuladoRespondido() => new()
        {
            SimuladoId = 1,
            NotaFinal = 600,
            Desempenho = new DesempenhoSimulado { QuantidadeDeQuestoes = 2, QuantidadeDeAcertos = 1 },
            Questoes = new List<SimuladoQuestao>
            {
                new()
                {
                    QuestaoId = 10,
                    Questao = new Questao
                    {
                        QuestaoId = 10,
                        Disciplina = "matematica",
                        Titulo = "Calcule a área do triângulo",
                        AlternativaCorreta = "A",
                        Alternativas = new List<Alternativa>
                        {
                            new() { AlternativaId = 1, Letra = "A", Texto = "certa", Correta = true },
                            new() { AlternativaId = 2, Letra = "B", Texto = "errada" }
                        }
                    }
                },
                new()
                {
                    QuestaoId = 11,
                    Questao = new Questao
                    {
                        QuestaoId = 11,
                        Disciplina = "linguagens",
                        Titulo = "Identifique a metáfora presente",
                        AlternativaCorreta = "C",
                        Alternativas = new List<Alternativa>
                        {
                            new() { AlternativaId = 3, Letra = "C", Texto = "certa", Correta = true }
                        }
                    }
                }
            },
            Respostas = new List<RespostaSimulado>
            {
                new() { QuestaoId = 10, Alternativa = new Alternativa { Letra = "B" } },
                new() { QuestaoId = 11, Alternativa = new Alternativa { Letra = "C" } }
            }
        };

        [Fact]
        public async Task GerarFeedbackAsync_EnviaDiagnosticoComHabilidadesErradas()
        {
            _http.ResponderChat("**Onde você está** ...");
            var sut = CriarSut();

            var feedback = await sut.GerarFeedbackAsync(SimuladoRespondido());

            Assert.Equal("**Onde você está** ...", feedback);
            var prompt = JObject.Parse(_http.CorposEnviados[0])["messages"][1]["content"].ToString();
            Assert.Contains("Acertos: 1/2", prompt);
            Assert.Contains("50%", prompt);
            Assert.Contains("intermediário", prompt);
            Assert.Contains("geometria (1 erro(s))", prompt);
        }

        [Fact]
        public async Task GerarFeedbackAsync_SemErros_InformaAusenciaDePadrao()
        {
            var simulado = SimuladoRespondido();
            simulado.Respostas[0].Alternativa.Letra = "A";
            _http.ResponderChat("ok");
            var sut = CriarSut();

            await sut.GerarFeedbackAsync(simulado);

            Assert.Contains("sem padrão claro", JObject.Parse(_http.CorposEnviados[0])["messages"][1]["content"].ToString());
        }

        [Fact]
        public async Task GerarFeedbackAsync_RespostaNula_RetornaMensagemPadrao()
        {
            _http.Responder("{\"choices\":null}");
            var sut = CriarSut();

            Assert.Equal("Erro ao gerar feedback.", await sut.GerarFeedbackAsync(SimuladoRespondido()));
        }

        [Fact]
        public async Task GerarPlanoAsync_PreservaDadosDoPlanoBase()
        {
            _http.ResponderChat("{\"HorasPorSemana\":10,\"Ativo\":true,\"UsuarioId\":99,\"PlanoMaterias\":[{\"HorasTotais\":10,\"HorasConcluidas\":0,\"Topicos\":[\"Tópico 1\"],\"Materia\":{\"Nome\":\"Matemática\",\"GeradaPorIA\":true,\"Cor\":\"#4F46E5\"}}]}");
            var sut = CriarSut();

            var planoBase = new PlanoEstudo
            {
                Titulo = "Plano ENEM",
                Objetivo = "Medicina",
                UsuarioId = 7,
                HorasPorSemana = 10,
                DataInicio = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DataFim = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            var plano = await sut.GerarPlanoAsync(planoBase);

            Assert.Equal("Plano ENEM", plano.Titulo);
            Assert.Equal("Medicina", plano.Objetivo);
            Assert.Equal(7, plano.UsuarioId);
            Assert.Equal(planoBase.DataInicio, plano.DataInicio);
            Assert.Equal(planoBase.DataFim, plano.DataFim);
            Assert.Equal("Matemática", Assert.Single(plano.PlanoMaterias).Materia.Nome);
        }

        [Fact]
        public async Task GerarPlanoAsync_DimensionaMateriasEHorasPeloPeriodo()
        {
            _http.ResponderChat("{\"PlanoMaterias\":[]}");
            var sut = CriarSut();

            await sut.GerarPlanoAsync(new PlanoEstudo
            {
                Titulo = "Plano ENEM",
                Objetivo = "Medicina",
                UsuarioId = 7,
                HorasPorSemana = 10,
                DataInicio = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DataFim = new DateTime(2026, 3, 26, 0, 0, 0, DateTimeKind.Utc)
            });

            var system = JObject.Parse(_http.CorposEnviados[0])["messages"][0]["content"].ToString();
            Assert.Contains("O plano deve ter 6 materias", system);
            Assert.Contains("120 horas totais", system);
        }

        [Fact]
        public async Task GerarPlanoAsync_RespostaNula_LancaExcecao()
        {
            _http.Responder("{\"choices\":null}");
            var sut = CriarSut();

            await Assert.ThrowsAsync<Exception>(() => sut.GerarPlanoAsync(new PlanoEstudo
            {
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddMonths(3)
            }));
        }

        [Fact]
        public async Task EnviarMensagensAsync_DelegaParaOGroq()
        {
            _http.ResponderChat("<think>rascunho</think>resposta");
            var sut = CriarSut();

            var mensagem = await sut.EnviarMensagensAsync(new ChatRequest
            {
                model = "qwen/qwen3-32b",
                messages = new List<Message> { new() { role = "user", content = "oi" } }
            });

            Assert.Equal("resposta", mensagem.content);
        }

        [Fact]
        public async Task GerarExplicacoesAsync_RemoveCercaDeMarkdownEDesserializa()
        {
            _http.ResponderChat("```json\n[{\"QuestaoId\":10,\"Explicacao\":\"A alternativa A está certa porque...\"}]\n```");
            var sut = CriarSut();

            var explicacoes = await sut.GerarExplicacoesAsync(SimuladoRespondido().Questoes);

            var explicacao = Assert.Single(explicacoes);
            Assert.Equal(10, explicacao.QuestaoId);
            Assert.StartsWith("A alternativa A", explicacao.Explicacao);
        }

        [Fact]
        public async Task GerarExplicacoesAsync_RespostaNula_LancaExcecao()
        {
            _http.Responder("{\"choices\":null}");
            var sut = CriarSut();

            await Assert.ThrowsAsync<Exception>(
                () => sut.GerarExplicacoesAsync(SimuladoRespondido().Questoes));
        }

        [Fact]
        public async Task GerarMateriaisAsync_ResolveUrlDeCadaMaterial()
        {
            _http.ResponderChat("{\"materiais\":[{\"titulo\":\"Geometria com Ferretto\",\"tipo\":\"Videoaula\",\"area\":\"Matemática\",\"motivo\":\"você errou geometria\",\"termoBusca\":\"ferretto geometria plana\"},{\"titulo\":\"Interpretação de texto\",\"tipo\":\"Videoaula\",\"area\":\"Linguagens\",\"motivo\":\"treino geral\"}]}");
            _busca.Setup(b => b.ResolverUrlAsync(It.IsAny<string>()))
                .ReturnsAsync((string termo) => $"https://youtube.com/{termo}");
            var sut = CriarSut();

            var materiais = await sut.GerarMateriaisAsync(SimuladoRespondido());

            Assert.Equal(2, materiais.Count);
            Assert.All(materiais, m => Assert.Equal("youtube", m.Plataforma));
            Assert.Equal("https://youtube.com/ferretto geometria plana", materiais[0].Url);
            Assert.Equal("https://youtube.com/Interpretação de texto", materiais[1].Url);
        }

        [Fact]
        public async Task GerarMateriaisAsync_RespostaVazia_RetornaListaVazia()
        {
            _http.ResponderChat("   ");
            var sut = CriarSut();

            Assert.Empty(await sut.GerarMateriaisAsync(SimuladoRespondido()));
            _busca.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GerarMateriaisAsync_SemMateriaisNoJson_RetornaListaVazia()
        {
            _http.ResponderChat("{\"materiais\":null}");
            var sut = CriarSut();

            Assert.Empty(await sut.GerarMateriaisAsync(SimuladoRespondido()));
        }
    }
}
