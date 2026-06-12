using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Learnly.Application.Applications;
using Learnly.Domain.Entities.Simulados;
using Learnly.Domain.Exceptions.Simulados;
using Learnly.Application.Interfaces;
using Learnly.Repository.Interfaces;
using Moq;
using Xunit;

namespace Learnly.Tests
{
    public class SimuladoAplicacaoTests
    {
        private readonly Mock<ISimuladoRepositorio> _simuladoRepo = new();
        private readonly Mock<IUsuarioRepositorio> _usuarioRepo = new();
        private readonly Mock<IIAService> _iaService = new();
        private readonly Mock<IValidator<Simulado>> _validator = new();

        private SimuladoAplicacao CriarSut() => new(
            _simuladoRepo.Object,
            _usuarioRepo.Object,
            _iaService.Object,
            _validator.Object);

        [Fact]
        public async Task ResponderSimulado_SemRespostas_LancaRespostasNaoInformadas()
        {
            var sut = CriarSut();

            await Assert.ThrowsAsync<RespostasNaoInformadasException>(
                () => sut.ResponderSimulado(1, new List<RespostaSimulado>(), 1));
        }

        [Fact]
        public async Task ResponderSimulado_SimuladoInexistente_LancaNaoEncontrado()
        {
            _simuladoRepo.Setup(r => r.Obter(It.IsAny<int>())).ReturnsAsync((Simulado)null);
            var sut = CriarSut();

            var respostas = new List<RespostaSimulado> { new() { QuestaoId = 1, AlternativaId = 1 } };

            await Assert.ThrowsAsync<SimuladoNaoEncontradoException>(
                () => sut.ResponderSimulado(1, respostas, 1));
        }

        [Fact]
        public async Task ResponderSimulado_DeOutroUsuario_LancaNaoAutorizado()
        {
            _simuladoRepo.Setup(r => r.Obter(It.IsAny<int>()))
                .ReturnsAsync(new Simulado { SimuladoId = 1, UsuarioId = 1 });
            var sut = CriarSut();

            var respostas = new List<RespostaSimulado> { new() { QuestaoId = 1, AlternativaId = 1 } };

            await Assert.ThrowsAsync<SimuladoNaoAutorizadoException>(
                () => sut.ResponderSimulado(1, respostas, 999));
        }

        [Fact]
        public async Task Obter_SimuladoInexistente_LancaNaoEncontrado()
        {
            _simuladoRepo.Setup(r => r.Obter(It.IsAny<int>())).ReturnsAsync((Simulado)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<SimuladoNaoEncontradoException>(() => sut.Obter(1, 1));
        }

        [Fact]
        public async Task Obter_DeOutroUsuario_LancaNaoAutorizado()
        {
            _simuladoRepo.Setup(r => r.Obter(It.IsAny<int>()))
                .ReturnsAsync(new Simulado { SimuladoId = 1, UsuarioId = 1 });
            var sut = CriarSut();

            await Assert.ThrowsAsync<SimuladoNaoAutorizadoException>(() => sut.Obter(1, 999));
        }

        [Fact]
        public async Task GerarSimulado_Nulo_LancaArgumentException()
        {
            var sut = CriarSut();

            await Assert.ThrowsAsync<ArgumentException>(
                () => sut.GerarSimulado(null, new List<string>(), 10));
        }

        [Fact]
        public async Task ResponderSimulado_CalculaNotaEDesempenho()
        {
            var simulado = new Simulado
            {
                SimuladoId = 1,
                UsuarioId = 1,
                Questoes = Enumerable.Range(1, 4).Select(i => new SimuladoQuestao
                {
                    QuestaoId = i,
                    Questao = new Questao
                    {
                        QuestaoId = i,
                        AlternativaCorreta = "A",
                        Arquivos = "img.png"
                    }
                }).ToList()
            };

            _simuladoRepo.Setup(r => r.Obter(1)).ReturnsAsync(simulado);
            _simuladoRepo.Setup(r => r.ObterQuestao(It.IsAny<int>()))
                .ReturnsAsync((int id) => new Questao { QuestaoId = id, AlternativaCorreta = "A" });
            _simuladoRepo.Setup(r => r.ObterAlternativa(It.IsAny<int>()))
                .ReturnsAsync((int id) => new Alternativa
                {
                    AlternativaId = id,
                    Letra = id <= 2 ? "A" : "B"
                });

            _iaService.Setup(s => s.GerarFeedbackAsync(It.IsAny<Simulado>())).ReturnsAsync("feedback");
            _iaService.Setup(s => s.GerarExplicacoesAsync(
                    It.IsAny<List<SimuladoQuestao>>(),
                    It.IsAny<Dictionary<int, RespostaSimulado>>()))
                .ReturnsAsync(new List<ExplicacaoQuestao>());
            _iaService.Setup(s => s.GerarMateriaisAsync(It.IsAny<Simulado>()))
                .ReturnsAsync(new List<MaterialRecomendado>());

            var respostas = Enumerable.Range(1, 4)
                .Select(i => new RespostaSimulado { QuestaoId = i, AlternativaId = i })
                .ToList();

            var sut = CriarSut();
            var resultado = await sut.ResponderSimulado(1, respostas, 1);

            Assert.Equal(5.00m, resultado.NotaFinal);
            Assert.Equal(2, resultado.Desempenho.QuantidadeDeAcertos);
            Assert.Equal(4, resultado.Desempenho.QuantidadeDeQuestoes);
            Assert.Equal("feedback", resultado.Desempenho.Feedback);
            _simuladoRepo.Verify(r => r.ResponderSimulado(simulado), Times.Once);
        }

        [Fact]
        public async Task ResponderSimulado_ErroNosMateriais_NaoQuebraACorrecao()
        {
            var simulado = new Simulado
            {
                SimuladoId = 1,
                UsuarioId = 1,
                Questoes = new List<SimuladoQuestao>
                {
                    new()
                    {
                        QuestaoId = 1,
                        Questao = new Questao { QuestaoId = 1, AlternativaCorreta = "A", Arquivos = "img.png" }
                    }
                }
            };

            _simuladoRepo.Setup(r => r.Obter(1)).ReturnsAsync(simulado);
            _simuladoRepo.Setup(r => r.ObterQuestao(1))
                .ReturnsAsync(new Questao { QuestaoId = 1, AlternativaCorreta = "A" });
            _simuladoRepo.Setup(r => r.ObterAlternativa(1))
                .ReturnsAsync(new Alternativa { AlternativaId = 1, Letra = "A" });

            _iaService.Setup(s => s.GerarFeedbackAsync(It.IsAny<Simulado>())).ReturnsAsync("feedback");
            _iaService.Setup(s => s.GerarExplicacoesAsync(
                    It.IsAny<List<SimuladoQuestao>>(),
                    It.IsAny<Dictionary<int, RespostaSimulado>>()))
                .ReturnsAsync(new List<ExplicacaoQuestao>());
            _iaService.Setup(s => s.GerarMateriaisAsync(It.IsAny<Simulado>()))
                .ThrowsAsync(new HttpRequestException("YouTube fora do ar"));

            var respostas = new List<RespostaSimulado> { new() { QuestaoId = 1, AlternativaId = 1 } };

            var sut = CriarSut();
            var resultado = await sut.ResponderSimulado(1, respostas, 1);

            Assert.Equal(10.00m, resultado.NotaFinal);
            Assert.Empty(resultado.MateriaisRecomendados);
        }
    }
}
