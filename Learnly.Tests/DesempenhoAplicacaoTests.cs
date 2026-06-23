using Learnly.Application.Applications;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Simulados;
using Learnly.Repository.Interfaces;
using Moq;

namespace Learnly.Tests
{
    public class DesempenhoAplicacaoTests
    {
        private readonly Mock<ISimuladoRepositorio> _simuladoRepo = new();
        private readonly Mock<IPlanoRepositorio> _planoRepo = new();
        private readonly Mock<IHoraLancadaRepositorio> _horaRepo = new();
        private readonly Mock<IUsuarioRepositorio> _usuarioRepo = new();
        private readonly Mock<IRedacaoRepositorio> _redacaoRepo = new();

        private DesempenhoAplicacao CriarSut()
        {
            _usuarioRepo.Setup(r => r.Obter(1, true)).ReturnsAsync(new Usuario { Id = 1 });
            _horaRepo.Setup(r => r.ListarPeriodoAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<HoraLancada>());
            _horaRepo.Setup(r => r.SomarHorasPeriodoAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(0);
            _horaRepo.Setup(r => r.ListarDatasComLancamento(1)).ReturnsAsync(new List<DateTime>());
            _simuladoRepo.Setup(r => r.ListarRespostasComQuestao(1)).ReturnsAsync(new List<RespostaSimulado>());
            _simuladoRepo.Setup(r => r.ListarNotas(1)).ReturnsAsync(new List<Simulado>());
            _simuladoRepo.Setup(r => r.ContarTotal(1)).ReturnsAsync(0);
            _planoRepo.Setup(r => r.ObterPlanoAtivo(1)).ReturnsAsync((Learnly.Domain.Entities.PlanoEstudo)null);
            _redacaoRepo.Setup(r => r.Listar(1)).ReturnsAsync(new List<Learnly.Domain.Entities.Redacoes.Redacao>());

            return new(
                _simuladoRepo.Object,
                _planoRepo.Object,
                _horaRepo.Object,
                _usuarioRepo.Object,
                _redacaoRepo.Object);
        }

        [Fact]
        public async Task ObterDashboard_DiasConsecutivos_CalculaSequencia()
        {
            var hoje = DateTime.UtcNow.Date;
            var sut = CriarSut();
            _horaRepo.Setup(r => r.ListarDatasComLancamento(1)).ReturnsAsync(new List<DateTime>
            {
                hoje, hoje.AddDays(-1), hoje.AddDays(-2)
            });

            var dash = await sut.ObterDashboard(1);

            Assert.Equal(3, dash.SequenciaDias);
            Assert.Equal(3, dash.MelhorSequencia);
        }

        [Fact]
        public async Task ObterDashboard_SequenciaQuebrada_SeparaAtualDeMelhor()
        {
            var hoje = DateTime.UtcNow.Date;
            var sut = CriarSut();
            _horaRepo.Setup(r => r.ListarDatasComLancamento(1)).ReturnsAsync(new List<DateTime>
            {
                hoje, hoje.AddDays(-1),
                hoje.AddDays(-5), hoje.AddDays(-6), hoje.AddDays(-7)
            });

            var dash = await sut.ObterDashboard(1);

            Assert.Equal(2, dash.SequenciaDias);
            Assert.Equal(3, dash.MelhorSequencia);
        }

        [Fact]
        public async Task ObterDashboard_SemEstudoHojeNemOntem_ZeraSequenciaAtual()
        {
            var hoje = DateTime.UtcNow.Date;
            var sut = CriarSut();
            _horaRepo.Setup(r => r.ListarDatasComLancamento(1)).ReturnsAsync(new List<DateTime>
            {
                hoje.AddDays(-3)
            });

            var dash = await sut.ObterDashboard(1);

            Assert.Equal(0, dash.SequenciaDias);
            Assert.Equal(1, dash.MelhorSequencia);
        }

        [Fact]
        public async Task ObterDashboard_MontaMapaCalorDe28DiasEHorasDaSemana()
        {
            var hoje = DateTime.UtcNow.Date;
            var sut = CriarSut();
            _horaRepo.Setup(r => r.ListarPeriodoAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<HoraLancada>
                {
                    new() { Data = hoje, QuantdadeHoras = 2 },
                    new() { Data = hoje.AddDays(-10), QuantdadeHoras = 3 }
                });

            var dash = await sut.ObterDashboard(1);

            Assert.Equal(28, dash.MapaCalor.Count);
            Assert.Equal(7, dash.HorasPorDia.Count);
            Assert.Equal(5, dash.MapaCalor.Sum(d => d.Horas));
            Assert.Equal(2, dash.HorasPorDia.Last().Horas);
            Assert.Equal(2, dash.HorasEstudadasSemana);
        }

        [Fact]
        public async Task ObterDashboard_AgrupaAcertosPorDisciplina()
        {
            var sut = CriarSut();
            _simuladoRepo.Setup(r => r.ListarRespostasComQuestao(1)).ReturnsAsync(new List<RespostaSimulado>
            {
                new()
                {
                    Questao = new Questao { Disciplina = "Matemática", AlternativaCorreta = "A" },
                    Alternativa = new Alternativa { Letra = "A" }
                },
                new()
                {
                    Questao = new Questao { Disciplina = "Matemática", AlternativaCorreta = "A" },
                    Alternativa = new Alternativa { Letra = "B" }
                },
                new()
                {
                    Questao = new Questao { Disciplina = "Linguagens", AlternativaCorreta = "C" },
                    Alternativa = new Alternativa { Letra = "C" }
                }
            });

            var dash = await sut.ObterDashboard(1);

            Assert.Equal(3, dash.TotalQuestoesRespondidas);
            Assert.Equal(67, dash.TaxaAcertoGeral);

            var matematica = dash.DesempenhoPorDisciplina.Single(d => d.Disciplina == "Matemática");
            Assert.Equal(2, matematica.Respondidas);
            Assert.Equal(1, matematica.Acertos);
            Assert.Equal(50, matematica.PercentualAcerto);

            var linguagens = dash.DesempenhoPorDisciplina.Single(d => d.Disciplina == "Linguagens");
            Assert.Equal(100, linguagens.PercentualAcerto);
        }
    }
}
