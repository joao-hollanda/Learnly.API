using FluentValidation;
using Learnly.Application.Applications;
using Learnly.Application.DTOs;
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
    public class ChatAplicacaoTests
    {
        private readonly Mock<IChatRepositorio> _chatRepo = new();
        private readonly Mock<IGrupoRepositorio> _grupoRepo = new();
        private readonly Mock<IAmizadeRepositorio> _amizadeRepo = new();
        private readonly Mock<IUsuarioRepositorio> _usuarioRepo = new();

        private ChatAplicacao CriarSut() => new(
            _chatRepo.Object,
            _grupoRepo.Object,
            _amizadeRepo.Object,
            _usuarioRepo.Object,
            new MensagemValidator());

        private static Conversa ConversaDireta(int conversaId, params int[] participantes) => new()
        {
            ConversaId = conversaId,
            Tipo = ConversaTipo.Direta,
            Participantes = participantes.Select(id => new ConversaParticipante { UsuarioId = id }).ToList()
        };

        private static Conversa ConversaDeGrupo(int conversaId, int grupoId) => new()
        {
            ConversaId = conversaId,
            Tipo = ConversaTipo.Grupo,
            GrupoId = grupoId
        };

        [Fact]
        public async Task ObterOuCriarConversaDireta_ConsigoMesmo_LancaSolicitacaoInvalida()
        {
            var sut = CriarSut();

            await Assert.ThrowsAsync<SolicitacaoAmizadeInvalidaException>(
                () => sut.ObterOuCriarConversaDireta(1, 1));
        }

        [Fact]
        public async Task ObterOuCriarConversaDireta_SemAmizade_LancaSolicitacaoInvalida()
        {
            _amizadeRepo.Setup(r => r.ObterEntre(1, 2)).ReturnsAsync((Amizade)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<SolicitacaoAmizadeInvalidaException>(
                () => sut.ObterOuCriarConversaDireta(1, 2));
        }

        [Fact]
        public async Task ObterOuCriarConversaDireta_AmizadePendente_LancaSolicitacaoInvalida()
        {
            _amizadeRepo.Setup(r => r.ObterEntre(1, 2))
                .ReturnsAsync(new Amizade { Status = AmizadeStatus.Pendente });
            var sut = CriarSut();

            await Assert.ThrowsAsync<SolicitacaoAmizadeInvalidaException>(
                () => sut.ObterOuCriarConversaDireta(1, 2));
        }

        [Fact]
        public async Task ObterOuCriarConversaDireta_ConversaExistente_ReaproveitaId()
        {
            _amizadeRepo.Setup(r => r.ObterEntre(1, 2))
                .ReturnsAsync(new Amizade { Status = AmizadeStatus.Aceita });
            _chatRepo.Setup(r => r.ObterConversaDireta(1, 2))
                .ReturnsAsync(new Conversa { ConversaId = 55 });
            var sut = CriarSut();

            Assert.Equal(55, await sut.ObterOuCriarConversaDireta(1, 2));
            _chatRepo.Verify(r => r.CriarConversa(It.IsAny<Conversa>()), Times.Never);
        }

        [Fact]
        public async Task ObterOuCriarConversaDireta_SemConversa_CriaComOsDoisParticipantes()
        {
            _amizadeRepo.Setup(r => r.ObterEntre(1, 2))
                .ReturnsAsync(new Amizade { Status = AmizadeStatus.Aceita });
            _chatRepo.Setup(r => r.ObterConversaDireta(1, 2)).ReturnsAsync((Conversa)null);
            _chatRepo.Setup(r => r.CriarConversa(It.IsAny<Conversa>()))
                .Callback<Conversa>(c => c.ConversaId = 55)
                .Returns(Task.CompletedTask);
            var sut = CriarSut();

            Assert.Equal(55, await sut.ObterOuCriarConversaDireta(1, 2));
            _chatRepo.Verify(r => r.CriarConversa(It.Is<Conversa>(c =>
                c.Tipo == ConversaTipo.Direta &&
                c.Participantes.Count == 2 &&
                c.Participantes.Any(p => p.UsuarioId == 1) &&
                c.Participantes.Any(p => p.UsuarioId == 2))), Times.Once);
        }

        [Fact]
        public async Task ObterConversa_Inexistente_LancaConversaNaoEncontrada()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync((Conversa)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<ConversaNaoEncontradaException>(() => sut.ObterConversa(55, 1));
        }

        [Fact]
        public async Task ObterConversa_DiretaDeOutrosUsuarios_LancaAcessoNegado()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDireta(55, 2, 3));
            var sut = CriarSut();

            await Assert.ThrowsAsync<AcessoConversaNegadoException>(() => sut.ObterConversa(55, 1));
        }

        [Fact]
        public async Task ObterConversa_Direta_UsaNomeDoOutroComoTitulo()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDireta(55, 1, 2));
            _chatRepo.Setup(r => r.ListarParticipantes(55)).ReturnsAsync(new List<ConversaParticipante>
            {
                new() { UsuarioId = 1, Usuario = new Usuario { Nome = "João" } },
                new() { UsuarioId = 2, Usuario = new Usuario { Nome = "Maria" } }
            });
            var sut = CriarSut();

            var detalhe = await sut.ObterConversa(55, 1);

            Assert.Equal(ConversaTipo.Direta, detalhe.Tipo);
            Assert.Equal("Maria", detalhe.Titulo);
            Assert.Equal(2, detalhe.OutroUsuarioId);
        }

        [Fact]
        public async Task ObterConversa_GrupoSemVinculo_LancaAcessoNegado()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDeGrupo(55, 7));
            _grupoRepo.Setup(r => r.EhMembro(7, 1)).ReturnsAsync(false);
            var sut = CriarSut();

            await Assert.ThrowsAsync<AcessoConversaNegadoException>(() => sut.ObterConversa(55, 1));
        }

        [Fact]
        public async Task ObterConversa_GrupoRemovido_LancaGrupoNaoEncontrado()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDeGrupo(55, 7));
            _grupoRepo.Setup(r => r.EhMembro(7, 1)).ReturnsAsync(true);
            _grupoRepo.Setup(r => r.Obter(7)).ReturnsAsync((Grupo)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<GrupoNaoEncontradoException>(() => sut.ObterConversa(55, 1));
        }

        [Fact]
        public async Task ObterConversa_Grupo_RetornaMembrosOrdenados()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDeGrupo(55, 7));
            _grupoRepo.Setup(r => r.EhMembro(7, 1)).ReturnsAsync(true);
            _grupoRepo.Setup(r => r.Obter(7)).ReturnsAsync(new Grupo
            {
                GrupoId = 7,
                Nome = "Estudo ENEM",
                Membros = new List<GrupoMembro>
                {
                    new() { UsuarioId = 2, Papel = GrupoPapel.Membro, Usuario = new Usuario { Nome = "Zeca" } },
                    new() { UsuarioId = 1, Papel = GrupoPapel.Admin, Usuario = new Usuario { Nome = "Ana" } }
                }
            });
            var sut = CriarSut();

            var detalhe = await sut.ObterConversa(55, 1);

            Assert.Equal("Estudo ENEM", detalhe.Titulo);
            Assert.Equal(7, detalhe.GrupoId);
            Assert.Equal(new[] { "Ana", "Zeca" }, detalhe.Membros.Select(m => m.Nome));
        }

        [Fact]
        public async Task ListarMensagens_LimiteNaoInformado_UsaTrinta()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDireta(55, 1, 2));
            _chatRepo.Setup(r => r.ListarMensagens(55, null, 30)).ReturnsAsync(new List<Mensagem>
            {
                new()
                {
                    MensagemId = 1,
                    ConversaId = 55,
                    RemetenteId = 2,
                    Texto = "oi",
                    Remetente = new Usuario { Nome = "Maria" }
                }
            });
            var sut = CriarSut();

            var mensagens = await sut.ListarMensagens(55, 1, null, 0);

            var mensagem = Assert.Single(mensagens);
            Assert.Equal("oi", mensagem.Texto);
            Assert.Equal("Maria", mensagem.RemetenteNome);
        }

        [Fact]
        public async Task EnviarMensagem_TextoVazio_LancaArgumentException()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDireta(55, 1, 2));
            var sut = CriarSut();

            await Assert.ThrowsAsync<ArgumentException>(() => sut.EnviarMensagem(55, 1, new NovaMensagemDto
            {
                Texto = "   ",
                Tipo = MensagemTipo.Texto
            }));
        }

        [Fact]
        public async Task EnviarMensagem_TextoAcimaDoLimite_LancaValidationException()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDireta(55, 1, 2));
            var sut = CriarSut();

            await Assert.ThrowsAsync<ValidationException>(() => sut.EnviarMensagem(55, 1, new NovaMensagemDto
            {
                Texto = new string('a', 4001),
                Tipo = MensagemTipo.Texto
            }));
        }

        [Fact]
        public async Task EnviarMensagem_Valida_PersisteEAtualizaConversa()
        {
            var conversa = ConversaDireta(55, 1, 2);
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(conversa);
            _chatRepo.Setup(r => r.AdicionarMensagem(It.IsAny<Mensagem>()))
                .Callback<Mensagem>(m => m.MensagemId = 300)
                .Returns(Task.CompletedTask);
            _chatRepo.Setup(r => r.ObterParticipante(55, 1))
                .ReturnsAsync(new ConversaParticipante { ConversaId = 55, UsuarioId = 1 });
            _usuarioRepo.Setup(r => r.Obter(1, true)).ReturnsAsync(new Usuario { Id = 1, Nome = "João" });
            var sut = CriarSut();

            var dto = await sut.EnviarMensagem(55, 1, new NovaMensagemDto { Texto = "  olá  " });

            Assert.Equal(300, dto.MensagemId);
            Assert.Equal("olá", dto.Texto);
            Assert.Equal("João", dto.RemetenteNome);
            Assert.Equal(dto.DataEnvio, conversa.UltimaMensagemData);
            _chatRepo.Verify(r => r.AtualizarConversa(conversa), Times.Once);
            _chatRepo.Verify(r => r.AtualizarParticipante(It.IsAny<ConversaParticipante>()), Times.Once);
        }

        [Fact]
        public async Task EnviarMensagem_AnexoSemTexto_NaoExigeTexto()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDireta(55, 1, 2));
            _chatRepo.Setup(r => r.ObterParticipante(55, 1)).ReturnsAsync((ConversaParticipante)null);
            var sut = CriarSut();

            var dto = await sut.EnviarMensagem(55, 1, new NovaMensagemDto
            {
                Tipo = MensagemTipo.Plano,
                AnexoRefId = 9,
                AnexoPayload = "{\"titulo\":\"Plano\"}"
            });

            Assert.Equal(MensagemTipo.Plano, dto.Tipo);
            Assert.Equal(9, dto.AnexoRefId);
            _chatRepo.Verify(r => r.AtualizarParticipante(It.IsAny<ConversaParticipante>()), Times.Never);
        }

        [Fact]
        public async Task MarcarLida_ConversaInexistente_LancaConversaNaoEncontrada()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync((Conversa)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<ConversaNaoEncontradaException>(() => sut.MarcarLida(55, 1));
        }

        [Fact]
        public async Task MarcarLida_Direta_AtualizaUltimaLeituraDoParticipante()
        {
            var participante = new ConversaParticipante { ConversaId = 55, UsuarioId = 1 };
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDireta(55, 1, 2));
            _chatRepo.Setup(r => r.ObterParticipante(55, 1)).ReturnsAsync(participante);
            var sut = CriarSut();

            await sut.MarcarLida(55, 1);

            Assert.NotNull(participante.UltimaLeitura);
            _chatRepo.Verify(r => r.AtualizarParticipante(participante), Times.Once);
        }

        [Fact]
        public async Task MarcarLida_Grupo_AtualizaUltimaLeituraDoMembro()
        {
            var membro = new GrupoMembro { GrupoId = 7, UsuarioId = 1 };
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDeGrupo(55, 7));
            _grupoRepo.Setup(r => r.ObterMembro(7, 1)).ReturnsAsync(membro);
            var sut = CriarSut();

            await sut.MarcarLida(55, 1);

            Assert.NotNull(membro.UltimaLeitura);
            _grupoRepo.Verify(r => r.AtualizarMembro(membro), Times.Once);
        }

        [Fact]
        public async Task MarcarLida_GrupoSemMembro_NaoAtualiza()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDeGrupo(55, 7));
            _grupoRepo.Setup(r => r.ObterMembro(7, 1)).ReturnsAsync((GrupoMembro)null);
            var sut = CriarSut();

            await sut.MarcarLida(55, 1);

            _grupoRepo.Verify(r => r.AtualizarMembro(It.IsAny<GrupoMembro>()), Times.Never);
        }

        [Fact]
        public async Task ListarConversas_OrdenaDaMaisRecenteParaAMaisAntiga()
        {
            var agora = DateTime.UtcNow;
            _chatRepo.Setup(r => r.ListarConversasDiretas(1)).ReturnsAsync(new List<Conversa>
            {
                new()
                {
                    ConversaId = 55,
                    Tipo = ConversaTipo.Direta,
                    Participantes = new List<ConversaParticipante>
                    {
                        new() { UsuarioId = 1, UltimaLeitura = agora.AddHours(-2) },
                        new() { UsuarioId = 2, Usuario = new Usuario { Nome = "Maria" } }
                    }
                }
            });
            _chatRepo.Setup(r => r.ObterUltimaMensagem(55))
                .ReturnsAsync(new Mensagem { Texto = "bom dia", DataEnvio = agora.AddHours(-1) });
            _chatRepo.Setup(r => r.ContarNaoLidas(55, 1, It.IsAny<DateTime?>())).ReturnsAsync(3);

            _grupoRepo.Setup(r => r.ListarPorUsuario(1)).ReturnsAsync(new List<Grupo>
            {
                new()
                {
                    GrupoId = 7,
                    Nome = "Estudo ENEM",
                    Membros = new List<GrupoMembro> { new() { UsuarioId = 1 } }
                }
            });
            _chatRepo.Setup(r => r.ObterConversaDoGrupo(7)).ReturnsAsync(new Conversa { ConversaId = 66 });
            _chatRepo.Setup(r => r.ObterUltimaMensagem(66))
                .ReturnsAsync(new Mensagem { Tipo = MensagemTipo.Simulado, DataEnvio = agora });
            _chatRepo.Setup(r => r.ContarNaoLidas(66, 1, It.IsAny<DateTime?>())).ReturnsAsync(0);
            var sut = CriarSut();

            var conversas = await sut.ListarConversas(1);

            Assert.Equal(new[] { 66, 55 }, conversas.Select(c => c.ConversaId));
            Assert.Equal("Compartilhou um simulado", conversas[0].UltimaMensagem);
            Assert.Equal("Estudo ENEM", conversas[0].Titulo);
            Assert.Equal("bom dia", conversas[1].UltimaMensagem);
            Assert.Equal("Maria", conversas[1].Titulo);
            Assert.Equal(3, conversas[1].NaoLidas);
        }

        [Theory]
        [InlineData(MensagemTipo.Plano, "Compartilhou um plano de estudo")]
        [InlineData(MensagemTipo.SessaoIA, "Compartilhou uma conversa com a IA")]
        public async Task ListarConversas_ResumeAnexosPorTipo(MensagemTipo tipo, string esperado)
        {
            _chatRepo.Setup(r => r.ListarConversasDiretas(1)).ReturnsAsync(new List<Conversa>
            {
                new()
                {
                    ConversaId = 55,
                    Tipo = ConversaTipo.Direta,
                    Participantes = new List<ConversaParticipante> { new() { UsuarioId = 1 } }
                }
            });
            _chatRepo.Setup(r => r.ObterUltimaMensagem(55))
                .ReturnsAsync(new Mensagem { Tipo = tipo, DataEnvio = DateTime.UtcNow });
            _grupoRepo.Setup(r => r.ListarPorUsuario(1)).ReturnsAsync(new List<Grupo>());
            var sut = CriarSut();

            var conversas = await sut.ListarConversas(1);

            Assert.Equal(esperado, Assert.Single(conversas).UltimaMensagem);
        }

        [Fact]
        public async Task ListarConversas_GrupoSemConversa_EhIgnorado()
        {
            _chatRepo.Setup(r => r.ListarConversasDiretas(1)).ReturnsAsync(new List<Conversa>());
            _grupoRepo.Setup(r => r.ListarPorUsuario(1)).ReturnsAsync(new List<Grupo>
            {
                new() { GrupoId = 7, Membros = new List<GrupoMembro>() }
            });
            _chatRepo.Setup(r => r.ObterConversaDoGrupo(7)).ReturnsAsync((Conversa)null);
            var sut = CriarSut();

            Assert.Empty(await sut.ListarConversas(1));
        }

        [Fact]
        public async Task ListarIdsConversas_JuntaDiretasEGruposSemRepetir()
        {
            _chatRepo.Setup(r => r.ListarConversasDiretas(1)).ReturnsAsync(new List<Conversa>
            {
                new() { ConversaId = 55 }
            });
            _grupoRepo.Setup(r => r.ListarPorUsuario(1)).ReturnsAsync(new List<Grupo>
            {
                new() { GrupoId = 7 },
                new() { GrupoId = 8 }
            });
            _chatRepo.Setup(r => r.ObterConversaDoGrupo(7)).ReturnsAsync(new Conversa { ConversaId = 66 });
            _chatRepo.Setup(r => r.ObterConversaDoGrupo(8)).ReturnsAsync((Conversa)null);
            var sut = CriarSut();

            Assert.Equal(new[] { 55, 66 }, await sut.ListarIdsConversas(1));
        }

        [Fact]
        public async Task ListarDestinatarios_ConversaInexistente_RetornaVazio()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync((Conversa)null);
            var sut = CriarSut();

            Assert.Empty(await sut.ListarDestinatarios(55, 1));
        }

        [Fact]
        public async Task ListarDestinatarios_Direta_ExcluiORemetente()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDireta(55, 1, 2));
            _chatRepo.Setup(r => r.ListarParticipantes(55)).ReturnsAsync(new List<ConversaParticipante>
            {
                new() { UsuarioId = 1 },
                new() { UsuarioId = 2 }
            });
            var sut = CriarSut();

            Assert.Equal(new[] { 2 }, await sut.ListarDestinatarios(55, 1));
        }

        [Fact]
        public async Task ListarDestinatarios_Grupo_ExcluiORemetente()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDeGrupo(55, 7));
            _grupoRepo.Setup(r => r.Obter(7)).ReturnsAsync(new Grupo
            {
                GrupoId = 7,
                Membros = new List<GrupoMembro>
                {
                    new() { UsuarioId = 1 },
                    new() { UsuarioId = 2 },
                    new() { UsuarioId = 3 }
                }
            });
            var sut = CriarSut();

            Assert.Equal(new[] { 2, 3 }, await sut.ListarDestinatarios(55, 1));
        }

        [Fact]
        public async Task ListarDestinatarios_GrupoRemovido_RetornaVazio()
        {
            _chatRepo.Setup(r => r.ObterConversa(55)).ReturnsAsync(ConversaDeGrupo(55, 7));
            _grupoRepo.Setup(r => r.Obter(7)).ReturnsAsync((Grupo)null);
            var sut = CriarSut();

            Assert.Empty(await sut.ListarDestinatarios(55, 1));
        }
    }
}
