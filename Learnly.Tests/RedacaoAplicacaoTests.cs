using FluentValidation;
using Learnly.Application.Applications;
using Learnly.Application.Interfaces;
using Learnly.Application.Validators;
using Learnly.Domain.Entities.Redacoes;
using Learnly.Domain.Exceptions.Comuns;
using Learnly.Domain.Exceptions.Redacoes;
using Learnly.Repository.Interfaces;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Learnly.Tests
{
    public class RedacaoAplicacaoTests
    {
        private readonly Mock<IRedacaoRepositorio> _redacaoRepo = new();
        private readonly Mock<IRedacaoIAService> _iaService = new();

        private RedacaoAplicacao CriarSut() => new(_redacaoRepo.Object, _iaService.Object, new RedacaoValidator());

        private static string TextoValido() => new('a', 200);

        [Theory]
        [InlineData((byte[])null)]
        [InlineData(new byte[0])]
        public async Task Transcrever_ImagemAusente_LancaRegraDeNegocio(byte[] imagem)
        {
            var sut = CriarSut();

            await Assert.ThrowsAsync<RegraDeNegocioException>(() => sut.Transcrever(imagem, "image/png"));
        }

        [Fact]
        public async Task Transcrever_ImagemValida_DelegaParaIA()
        {
            var imagem = new byte[] { 1, 2, 3 };
            _iaService.Setup(s => s.TranscreverImagemAsync(imagem, "image/png")).ReturnsAsync("texto transcrito");
            var sut = CriarSut();

            Assert.Equal("texto transcrito", await sut.Transcrever(imagem, "image/png"));
        }

        [Fact]
        public async Task GerarTema_DelegaParaIA()
        {
            _iaService.Setup(s => s.GerarTemaAsync()).ReturnsAsync("Tema do ENEM");
            var sut = CriarSut();

            Assert.Equal("Tema do ENEM", await sut.GerarTema());
        }

        [Fact]
        public async Task Corrigir_TextoCurto_LancaValidationException()
        {
            var sut = CriarSut();

            await Assert.ThrowsAsync<ValidationException>(() => sut.Corrigir(1, "Tema", "muito curto"));
            _iaService.Verify(s => s.CorrigirAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Corrigir_Valida_SomaCompetenciasEPersiste()
        {
            var correcao = new CorrecaoRedacao
            {
                ComentarioGeral = "Bom texto",
                Competencias = new List<CompetenciaNota>
                {
                    new() { Numero = 1, Nota = 200 },
                    new() { Numero = 2, Nota = 160 },
                    new() { Numero = 3, Nota = 120 },
                    new() { Numero = 4, Nota = 200 },
                    new() { Numero = 5, Nota = 80 }
                }
            };
            _iaService.Setup(s => s.CorrigirAsync("Tema", It.IsAny<string>())).ReturnsAsync(correcao);
            _redacaoRepo.Setup(r => r.Criar(It.IsAny<Redacao>())).ReturnsAsync(15);
            var sut = CriarSut();

            var redacao = await sut.Corrigir(1, "Tema", TextoValido());

            Assert.Equal(15, redacao.RedacaoId);
            Assert.Equal(200, redacao.NotaC1);
            Assert.Equal(160, redacao.NotaC2);
            Assert.Equal(120, redacao.NotaC3);
            Assert.Equal(200, redacao.NotaC4);
            Assert.Equal(80, redacao.NotaC5);
            Assert.Equal(760, redacao.NotaFinal);
            Assert.Equal(correcao.ComentarioGeral,
                JsonConvert.DeserializeObject<CorrecaoRedacao>(redacao.ComentariosJson).ComentarioGeral);
        }

        [Fact]
        public async Task Corrigir_CompetenciaAusente_ContaComoZero()
        {
            _iaService.Setup(s => s.CorrigirAsync("Tema", It.IsAny<string>())).ReturnsAsync(new CorrecaoRedacao
            {
                Competencias = new List<CompetenciaNota> { new() { Numero = 1, Nota = 200 } }
            });
            var sut = CriarSut();

            var redacao = await sut.Corrigir(1, "Tema", TextoValido());

            Assert.Equal(0, redacao.NotaC5);
            Assert.Equal(200, redacao.NotaFinal);
        }

        [Fact]
        public async Task Listar_DelegaParaRepositorio()
        {
            var redacoes = new List<Redacao> { new() { RedacaoId = 1 } };
            _redacaoRepo.Setup(r => r.Listar(1)).ReturnsAsync(redacoes);
            var sut = CriarSut();

            Assert.Same(redacoes, await sut.Listar(1));
        }

        [Fact]
        public async Task Obter_Inexistente_LancaRedacaoNaoEncontrada()
        {
            _redacaoRepo.Setup(r => r.Obter(15)).ReturnsAsync((Redacao)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<RedacaoNaoEncontradaException>(() => sut.Obter(15, 1));
        }

        [Fact]
        public async Task Obter_DeOutroUsuario_LancaNaoAutorizada()
        {
            _redacaoRepo.Setup(r => r.Obter(15)).ReturnsAsync(new Redacao { RedacaoId = 15, UsuarioId = 2 });
            var sut = CriarSut();

            await Assert.ThrowsAsync<RedacaoNaoAutorizadaException>(() => sut.Obter(15, 1));
        }

        [Fact]
        public async Task Obter_DoProprioUsuario_Retorna()
        {
            var redacao = new Redacao { RedacaoId = 15, UsuarioId = 1 };
            _redacaoRepo.Setup(r => r.Obter(15)).ReturnsAsync(redacao);
            var sut = CriarSut();

            Assert.Same(redacao, await sut.Obter(15, 1));
        }
    }
}
