using FluentValidation;
using Learnly.Application.Applications;
using Learnly.Application.Validators;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Social;
using Learnly.Domain.Enums;
using Learnly.Domain.Exceptions.Social;
using Learnly.Repository.Interfaces;
using Moq;
using Xunit;

namespace Learnly.Tests
{
    public class GrupoAplicacaoTests
    {
        private readonly Mock<IGrupoRepositorio> _grupoRepo = new();
        private readonly Mock<IChatRepositorio> _chatRepo = new();

        private GrupoAplicacao CriarSut() => new(_grupoRepo.Object, _chatRepo.Object, new GrupoValidator());

        [Fact]
        public async Task Criar_NomeVazio_LancaValidationException()
        {
            var sut = CriarSut();

            await Assert.ThrowsAsync<ValidationException>(() => sut.Criar(1, "   ", "descrição"));
        }

        [Fact]
        public async Task Criar_Valido_AdicionaCriadorComoAdminECriaConversa()
        {
            _grupoRepo.Setup(r => r.ChaveExiste(It.IsAny<string>())).ReturnsAsync(false);
            _grupoRepo.Setup(r => r.Criar(It.IsAny<Grupo>()))
                .Callback<Grupo>(g => g.GrupoId = 7)
                .Returns(Task.CompletedTask);
            _chatRepo.Setup(r => r.CriarConversa(It.IsAny<Conversa>()))
                .Callback<Conversa>(c => c.ConversaId = 99)
                .Returns(Task.CompletedTask);
            var sut = CriarSut();

            var dto = await sut.Criar(1, "  Estudo ENEM  ", "  turma da manhã  ");

            Assert.Equal(7, dto.GrupoId);
            Assert.Equal(99, dto.ConversaId);
            Assert.Equal("Estudo ENEM", dto.Nome);
            Assert.Equal("turma da manhã", dto.Descricao);
            Assert.Equal(1, dto.TotalMembros);
            Assert.True(dto.SouAdmin);
            Assert.Equal(6, dto.Chave.Length);
            _grupoRepo.Verify(r => r.AdicionarMembro(It.Is<GrupoMembro>(m =>
                m.GrupoId == 7 && m.UsuarioId == 1 && m.Papel == GrupoPapel.Admin)), Times.Once);
            _chatRepo.Verify(r => r.CriarConversa(It.Is<Conversa>(c =>
                c.Tipo == ConversaTipo.Grupo && c.GrupoId == 7)), Times.Once);
        }

        [Fact]
        public async Task Criar_ChaveJaExistente_GeraOutraChave()
        {
            _grupoRepo.SetupSequence(r => r.ChaveExiste(It.IsAny<string>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            var sut = CriarSut();

            var dto = await sut.Criar(1, "Estudo ENEM", null);

            Assert.Equal(6, dto.Chave.Length);
            _grupoRepo.Verify(r => r.ChaveExiste(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Entrar_ChaveInexistente_LancaChaveInvalida()
        {
            _grupoRepo.Setup(r => r.ObterPorChave(It.IsAny<string>())).ReturnsAsync((Grupo)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<ChaveGrupoInvalidaException>(() => sut.Entrar(1, "abc123"));
        }

        [Fact]
        public async Task Entrar_JaMembro_LancaJaMembro()
        {
            _grupoRepo.Setup(r => r.ObterPorChave("ABC123")).ReturnsAsync(new Grupo { GrupoId = 7 });
            _grupoRepo.Setup(r => r.EhMembro(7, 1)).ReturnsAsync(true);
            var sut = CriarSut();

            await Assert.ThrowsAsync<JaMembroDoGrupoException>(() => sut.Entrar(1, "abc123"));
        }

        [Fact]
        public async Task Entrar_ChaveValida_NormalizaChaveEAdicionaComoMembro()
        {
            _grupoRepo.Setup(r => r.ObterPorChave("ABC123")).ReturnsAsync(new Grupo
            {
                GrupoId = 7,
                Nome = "Estudo ENEM",
                CriadorId = 5,
                Membros = new List<GrupoMembro> { new() { UsuarioId = 5 } }
            });
            _grupoRepo.Setup(r => r.EhMembro(7, 1)).ReturnsAsync(false);
            _chatRepo.Setup(r => r.ObterConversaDoGrupo(7)).ReturnsAsync(new Conversa { ConversaId = 99 });
            var sut = CriarSut();

            var dto = await sut.Entrar(1, "  abc123  ");

            Assert.Equal(99, dto.ConversaId);
            Assert.Equal(2, dto.TotalMembros);
            Assert.False(dto.SouAdmin);
            _grupoRepo.Verify(r => r.AdicionarMembro(It.Is<GrupoMembro>(m =>
                m.GrupoId == 7 && m.UsuarioId == 1 && m.Papel == GrupoPapel.Membro)), Times.Once);
        }

        [Fact]
        public async Task Entrar_GrupoSemConversa_RetornaConversaZero()
        {
            _grupoRepo.Setup(r => r.ObterPorChave("ABC123")).ReturnsAsync(new Grupo { GrupoId = 7 });
            _grupoRepo.Setup(r => r.EhMembro(7, 1)).ReturnsAsync(false);
            _chatRepo.Setup(r => r.ObterConversaDoGrupo(7)).ReturnsAsync((Conversa)null);
            var sut = CriarSut();

            var dto = await sut.Entrar(1, "ABC123");

            Assert.Equal(0, dto.ConversaId);
            Assert.Equal(1, dto.TotalMembros);
        }

        [Fact]
        public async Task Sair_NaoMembro_LancaNaoMembro()
        {
            _grupoRepo.Setup(r => r.ObterMembro(7, 1)).ReturnsAsync((GrupoMembro)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<NaoMembroDoGrupoException>(() => sut.Sair(7, 1));
        }

        [Fact]
        public async Task Sair_Membro_RemoveVinculo()
        {
            var membro = new GrupoMembro { GrupoId = 7, UsuarioId = 1 };
            _grupoRepo.Setup(r => r.ObterMembro(7, 1)).ReturnsAsync(membro);
            var sut = CriarSut();

            await sut.Sair(7, 1);

            _grupoRepo.Verify(r => r.RemoverMembro(membro), Times.Once);
        }

        [Fact]
        public async Task ListarMeus_MarcaAdminEContaMembros()
        {
            _grupoRepo.Setup(r => r.ListarPorUsuario(1)).ReturnsAsync(new List<Grupo>
            {
                new()
                {
                    GrupoId = 7,
                    Nome = "Admin aqui",
                    Membros = new List<GrupoMembro>
                    {
                        new() { UsuarioId = 1, Papel = GrupoPapel.Admin },
                        new() { UsuarioId = 2, Papel = GrupoPapel.Membro }
                    }
                },
                new()
                {
                    GrupoId = 8,
                    Nome = "Só membro",
                    Membros = new List<GrupoMembro> { new() { UsuarioId = 1, Papel = GrupoPapel.Membro } }
                }
            });
            _chatRepo.Setup(r => r.ObterConversaDoGrupo(7)).ReturnsAsync(new Conversa { ConversaId = 99 });
            _chatRepo.Setup(r => r.ObterConversaDoGrupo(8)).ReturnsAsync((Conversa)null);
            var sut = CriarSut();

            var grupos = await sut.ListarMeus(1);

            Assert.Equal(2, grupos.Count);
            Assert.True(grupos[0].SouAdmin);
            Assert.Equal(2, grupos[0].TotalMembros);
            Assert.Equal(99, grupos[0].ConversaId);
            Assert.False(grupos[1].SouAdmin);
            Assert.Equal(0, grupos[1].ConversaId);
        }

        [Fact]
        public async Task Obter_GrupoInexistente_LancaGrupoNaoEncontrado()
        {
            _grupoRepo.Setup(r => r.Obter(7)).ReturnsAsync((Grupo)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<GrupoNaoEncontradoException>(() => sut.Obter(7, 1));
        }

        [Fact]
        public async Task Obter_UsuarioForaDoGrupo_LancaNaoMembro()
        {
            _grupoRepo.Setup(r => r.Obter(7)).ReturnsAsync(new Grupo
            {
                GrupoId = 7,
                Membros = new List<GrupoMembro> { new() { UsuarioId = 2 } }
            });
            var sut = CriarSut();

            await Assert.ThrowsAsync<NaoMembroDoGrupoException>(() => sut.Obter(7, 1));
        }

        [Fact]
        public async Task Obter_Membro_RetornaDetalheComMembrosOrdenados()
        {
            _grupoRepo.Setup(r => r.Obter(7)).ReturnsAsync(new Grupo
            {
                GrupoId = 7,
                Nome = "Estudo ENEM",
                CriadorId = 1,
                Membros = new List<GrupoMembro>
                {
                    new() { UsuarioId = 2, Papel = GrupoPapel.Membro, Usuario = new Usuario { Nome = "Zeca" } },
                    new() { UsuarioId = 1, Papel = GrupoPapel.Admin, Usuario = new Usuario { Nome = "Ana" } }
                }
            });
            _chatRepo.Setup(r => r.ObterConversaDoGrupo(7)).ReturnsAsync(new Conversa { ConversaId = 99 });
            var sut = CriarSut();

            var detalhe = await sut.Obter(7, 1);

            Assert.True(detalhe.SouAdmin);
            Assert.Equal(99, detalhe.ConversaId);
            Assert.Equal(new[] { "Ana", "Zeca" }, detalhe.Membros.Select(m => m.Nome));
            Assert.Equal("Admin", detalhe.Membros[0].Papel);
        }
    }
}
