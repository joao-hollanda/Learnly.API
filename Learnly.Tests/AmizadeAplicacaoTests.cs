using Learnly.Application.Applications;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Social;
using Learnly.Domain.Enums;
using Learnly.Domain.Exceptions.Social;
using Learnly.Repository.Interfaces;
using Moq;
using Xunit;

namespace Learnly.Tests
{
    public class AmizadeAplicacaoTests
    {
        private readonly Mock<IAmizadeRepositorio> _amizadeRepo = new();
        private readonly Mock<IUsuarioRepositorio> _usuarioRepo = new();

        private AmizadeAplicacao CriarSut() => new(_amizadeRepo.Object, _usuarioRepo.Object);

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task EnviarSolicitacao_TermoVazio_LancaSolicitacaoInvalida(string termo)
        {
            var sut = CriarSut();

            await Assert.ThrowsAsync<SolicitacaoAmizadeInvalidaException>(
                () => sut.EnviarSolicitacao(1, termo));
        }

        [Fact]
        public async Task EnviarSolicitacao_UsuarioInexistente_LancaSolicitacaoInvalida()
        {
            _usuarioRepo.Setup(r => r.ObterPorEmail(It.IsAny<string>())).ReturnsAsync((Usuario)null);
            _usuarioRepo.Setup(r => r.ObterPorNome(It.IsAny<string>())).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<SolicitacaoAmizadeInvalidaException>(
                () => sut.EnviarSolicitacao(1, "ninguem@learnly.com.br"));
        }

        [Fact]
        public async Task EnviarSolicitacao_ParaSiMesmo_LancaSolicitacaoInvalida()
        {
            _usuarioRepo.Setup(r => r.ObterPorEmail("eu@learnly.com.br"))
                .ReturnsAsync(new Usuario { Id = 1 });
            var sut = CriarSut();

            await Assert.ThrowsAsync<SolicitacaoAmizadeInvalidaException>(
                () => sut.EnviarSolicitacao(1, "eu@learnly.com.br"));
        }

        [Fact]
        public async Task EnviarSolicitacao_JaAmigos_LancaSolicitacaoInvalida()
        {
            _usuarioRepo.Setup(r => r.ObterPorEmail("amigo@learnly.com.br"))
                .ReturnsAsync(new Usuario { Id = 2 });
            _amizadeRepo.Setup(r => r.ObterEntre(1, 2))
                .ReturnsAsync(new Amizade { Status = AmizadeStatus.Aceita });
            var sut = CriarSut();

            var ex = await Assert.ThrowsAsync<SolicitacaoAmizadeInvalidaException>(
                () => sut.EnviarSolicitacao(1, "amigo@learnly.com.br"));

            Assert.Contains("já são amigos", ex.Message);
        }

        [Fact]
        public async Task EnviarSolicitacao_JaPendente_LancaSolicitacaoInvalida()
        {
            _usuarioRepo.Setup(r => r.ObterPorEmail("amigo@learnly.com.br"))
                .ReturnsAsync(new Usuario { Id = 2 });
            _amizadeRepo.Setup(r => r.ObterEntre(1, 2))
                .ReturnsAsync(new Amizade { Status = AmizadeStatus.Pendente });
            var sut = CriarSut();

            var ex = await Assert.ThrowsAsync<SolicitacaoAmizadeInvalidaException>(
                () => sut.EnviarSolicitacao(1, "amigo@learnly.com.br"));

            Assert.Contains("pendente", ex.Message);
        }

        [Fact]
        public async Task EnviarSolicitacao_BuscaPorNomeQuandoEmailNaoEncontra()
        {
            _usuarioRepo.Setup(r => r.ObterPorEmail("Maria")).ReturnsAsync((Usuario)null);
            _usuarioRepo.Setup(r => r.ObterPorNome("Maria"))
                .ReturnsAsync(new Usuario { Id = 2, Nome = "Maria", Email = "maria@learnly.com.br" });
            _amizadeRepo.Setup(r => r.ObterEntre(1, 2)).ReturnsAsync((Amizade)null);
            var sut = CriarSut();

            var dto = await sut.EnviarSolicitacao(1, "  Maria  ");

            Assert.Equal(2, dto.UsuarioId);
            Assert.Equal("Maria", dto.Nome);
            Assert.Equal("maria@learnly.com.br", dto.Email);
            _amizadeRepo.Verify(r => r.Criar(It.Is<Amizade>(a =>
                a.SolicitanteId == 1 &&
                a.DestinatarioId == 2 &&
                a.Status == AmizadeStatus.Pendente)), Times.Once);
        }

        [Fact]
        public async Task Aceitar_SolicitacaoInexistente_LancaNaoEncontrada()
        {
            _amizadeRepo.Setup(r => r.Obter(9)).ReturnsAsync((Amizade)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<AmizadeNaoEncontradaException>(() => sut.Aceitar(9, 1));
        }

        [Fact]
        public async Task Aceitar_UsuarioNaoEhDestinatario_LancaNaoEncontrada()
        {
            _amizadeRepo.Setup(r => r.Obter(9))
                .ReturnsAsync(new Amizade { DestinatarioId = 2 });
            var sut = CriarSut();

            await Assert.ThrowsAsync<AmizadeNaoEncontradaException>(() => sut.Aceitar(9, 3));
        }

        [Fact]
        public async Task Aceitar_SolicitacaoJaRespondida_LancaSolicitacaoInvalida()
        {
            _amizadeRepo.Setup(r => r.Obter(9))
                .ReturnsAsync(new Amizade { DestinatarioId = 2, Status = AmizadeStatus.Aceita });
            var sut = CriarSut();

            await Assert.ThrowsAsync<SolicitacaoAmizadeInvalidaException>(() => sut.Aceitar(9, 2));
        }

        [Fact]
        public async Task Aceitar_Pendente_MarcaAceitaERegistraResposta()
        {
            var amizade = new Amizade { DestinatarioId = 2, Status = AmizadeStatus.Pendente };
            _amizadeRepo.Setup(r => r.Obter(9)).ReturnsAsync(amizade);
            var sut = CriarSut();

            await sut.Aceitar(9, 2);

            Assert.Equal(AmizadeStatus.Aceita, amizade.Status);
            Assert.NotNull(amizade.DataResposta);
            _amizadeRepo.Verify(r => r.Atualizar(amizade), Times.Once);
        }

        [Fact]
        public async Task Recusar_UsuarioNaoEhDestinatario_LancaNaoEncontrada()
        {
            _amizadeRepo.Setup(r => r.Obter(9))
                .ReturnsAsync(new Amizade { DestinatarioId = 2 });
            var sut = CriarSut();

            await Assert.ThrowsAsync<AmizadeNaoEncontradaException>(() => sut.Recusar(9, 3));
        }

        [Fact]
        public async Task Recusar_Destinatario_RemoveAmizade()
        {
            var amizade = new Amizade { DestinatarioId = 2 };
            _amizadeRepo.Setup(r => r.Obter(9)).ReturnsAsync(amizade);
            var sut = CriarSut();

            await sut.Recusar(9, 2);

            _amizadeRepo.Verify(r => r.Remover(amizade), Times.Once);
        }

        [Fact]
        public async Task Remover_UsuarioForaDaAmizade_LancaNaoEncontrada()
        {
            _amizadeRepo.Setup(r => r.Obter(9))
                .ReturnsAsync(new Amizade { SolicitanteId = 1, DestinatarioId = 2 });
            var sut = CriarSut();

            await Assert.ThrowsAsync<AmizadeNaoEncontradaException>(() => sut.Remover(9, 3));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task Remover_QualquerLadoDaAmizade_Remove(int usuarioId)
        {
            var amizade = new Amizade { SolicitanteId = 1, DestinatarioId = 2 };
            _amizadeRepo.Setup(r => r.Obter(9)).ReturnsAsync(amizade);
            var sut = CriarSut();

            await sut.Remover(9, usuarioId);

            _amizadeRepo.Verify(r => r.Remover(amizade), Times.Once);
        }

        [Fact]
        public async Task ListarAmigos_RetornaOutroLadoOrdenadoPorNome()
        {
            _amizadeRepo.Setup(r => r.ListarAceitas(1)).ReturnsAsync(new List<Amizade>
            {
                new()
                {
                    AmizadeId = 10,
                    SolicitanteId = 1,
                    Destinatario = new Usuario { Id = 2, Nome = "Zeca", Email = "zeca@learnly.com.br" }
                },
                new()
                {
                    AmizadeId = 11,
                    SolicitanteId = 3,
                    DestinatarioId = 1,
                    Solicitante = new Usuario { Id = 3, Nome = "Ana", Email = "ana@learnly.com.br" }
                }
            });
            var sut = CriarSut();

            var amigos = await sut.ListarAmigos(1);

            Assert.Equal(new[] { "Ana", "Zeca" }, amigos.Select(a => a.Nome));
            Assert.Equal(3, amigos[0].UsuarioId);
            Assert.Equal(11, amigos[0].AmizadeId);
        }

        [Fact]
        public async Task ListarPendentesRecebidas_MapeiaSolicitante()
        {
            var enviadaEm = DateTime.UtcNow.AddDays(-1);
            _amizadeRepo.Setup(r => r.ListarPendentesRecebidas(1)).ReturnsAsync(new List<Amizade>
            {
                new()
                {
                    AmizadeId = 10,
                    DataSolicitacao = enviadaEm,
                    Solicitante = new Usuario { Id = 5, Nome = "Bia", Email = "bia@learnly.com.br" }
                }
            });
            var sut = CriarSut();

            var pendentes = await sut.ListarPendentesRecebidas(1);

            var pendente = Assert.Single(pendentes);
            Assert.Equal(5, pendente.UsuarioId);
            Assert.Equal("Bia", pendente.Nome);
            Assert.Equal(enviadaEm, pendente.DataSolicitacao);
        }

        [Fact]
        public async Task ListarPendentesEnviadas_MapeiaDestinatario()
        {
            _amizadeRepo.Setup(r => r.ListarPendentesEnviadas(1)).ReturnsAsync(new List<Amizade>
            {
                new()
                {
                    AmizadeId = 12,
                    Destinatario = new Usuario { Id = 8, Nome = "Caio", Email = "caio@learnly.com.br" }
                }
            });
            var sut = CriarSut();

            var pendentes = await sut.ListarPendentesEnviadas(1);

            var pendente = Assert.Single(pendentes);
            Assert.Equal(8, pendente.UsuarioId);
            Assert.Equal("Caio", pendente.Nome);
        }
    }
}
