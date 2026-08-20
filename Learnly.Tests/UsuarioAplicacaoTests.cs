using System.Security.Claims;
using FluentValidation;
using Learnly.Application.Applications;
using Learnly.Application.Interfaces;
using Learnly.Application.Validators;
using Learnly.Domain.Entities;
using Learnly.Domain.Exceptions.Autenticacao;
using Learnly.Domain.Exceptions.Usuarios;
using Learnly.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Learnly.Tests
{
    public class UsuarioAplicacaoTests
    {
        private readonly Mock<IUsuarioRepositorio> _usuarioRepo = new();
        private readonly Mock<IEmailService> _emailService = new();

        private readonly IConfiguration _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["jwt:secretKey"] = "chave-de-teste-suficientemente-longa-para-hmac-sha256-0123456789",
                ["jwt:issuer"] = "http://teste",
                ["jwt:audience"] = "http://teste",
                ["Email:FrontendUrl"] = "https://teste.learnly.com.br/"
            })
            .Build();

        private readonly LoginAplicacao _login;

        public UsuarioAplicacaoTests()
        {
            _login = new LoginAplicacao(_configuration);
        }

        private UsuarioAplicacao CriarSut(ILoginAplicacao? login = null) => new(
            _usuarioRepo.Object,
            new UsuarioValidator(),
            _emailService.Object,
            login ?? _login,
            _configuration);

        private static Usuario UsuarioValido() => new()
        {
            Id = 42,
            Nome = "João",
            Email = "joao@learnly.com.br",
            Senha = "Senha@123"
        };

        [Fact]
        public async Task Criar_UsuarioNulo_LancaArgumentNull()
        {
            var sut = CriarSut();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Criar(null));
        }

        [Fact]
        public async Task Criar_SenhaCurta_LancaValidationException()
        {
            var usuario = UsuarioValido();
            usuario.Senha = "123";
            var sut = CriarSut();

            await Assert.ThrowsAsync<ValidationException>(() => sut.Criar(usuario));
        }

        [Fact]
        public async Task Criar_EmailJaCadastrado_LancaEmailJaCadastrado()
        {
            _usuarioRepo.Setup(r => r.EmailEmUso("joao@learnly.com.br", 0)).ReturnsAsync(true);
            var sut = CriarSut();

            await Assert.ThrowsAsync<EmailJaCadastradoException>(() => sut.Criar(UsuarioValido()));
        }

        [Fact]
        public async Task Criar_Valido_HasheiaSenhaEEnviaConfirmacao()
        {
            _usuarioRepo.Setup(r => r.EmailEmUso(It.IsAny<string>(), 0)).ReturnsAsync(false);
            _usuarioRepo.Setup(r => r.Criar(It.IsAny<Usuario>())).ReturnsAsync(42);
            var usuario = UsuarioValido();
            var sut = CriarSut();

            var id = await sut.Criar(usuario);

            Assert.Equal(42, id);
            Assert.False(usuario.EmailConfirmado);
            Assert.NotEqual("Senha@123", usuario.Senha);
            Assert.True(BCrypt.Net.BCrypt.Verify("Senha@123", usuario.Senha));
            _emailService.Verify(s => s.EnviarConfirmacaoAsync(
                "joao@learnly.com.br",
                "João",
                It.Is<string>(link => link.StartsWith("https://teste.learnly.com.br/confirmar-email?token="))), Times.Once);
        }

        [Fact]
        public async Task Criar_FalhaAoEnviarEmail_NaoPropagaExcecao()
        {
            _usuarioRepo.Setup(r => r.EmailEmUso(It.IsAny<string>(), 0)).ReturnsAsync(false);
            _usuarioRepo.Setup(r => r.Criar(It.IsAny<Usuario>())).ReturnsAsync(42);
            _emailService
                .Setup(s => s.EnviarConfirmacaoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("provedor fora do ar"));
            var sut = CriarSut();

            var id = await sut.Criar(UsuarioValido());

            Assert.Equal(42, id);
        }

        [Fact]
        public async Task ConfirmarEmail_TokenIlegivel_LancaTokenInvalido()
        {
            var sut = CriarSut();

            await Assert.ThrowsAsync<TokenInvalidoException>(() => sut.ConfirmarEmail("nao-e-um-jwt"));
        }

        [Fact]
        public async Task ConfirmarEmail_TokenDeOutroTipo_LancaTokenInvalido()
        {
            var token = _login.GerarTokenAcao(42, "joao@learnly.com.br", "reset", TimeSpan.FromMinutes(30));
            var sut = CriarSut();

            await Assert.ThrowsAsync<TokenInvalidoException>(() => sut.ConfirmarEmail(token));
        }

        [Fact]
        public async Task ConfirmarEmail_IdNaoNumerico_LancaTokenInvalido()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("tipo", "confirmacao"),
                new Claim("id", "abc")
            }));
            var login = new Mock<ILoginAplicacao>();
            login.Setup(l => l.ValidarToken("token")).Returns(principal);
            var sut = CriarSut(login.Object);

            await Assert.ThrowsAsync<TokenInvalidoException>(() => sut.ConfirmarEmail("token"));
        }

        [Fact]
        public async Task ConfirmarEmail_UsuarioInexistente_LancaTokenInvalido()
        {
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync((Usuario)null);
            var token = _login.GerarTokenAcao(42, "joao@learnly.com.br", "confirmacao", TimeSpan.FromHours(24));
            var sut = CriarSut();

            await Assert.ThrowsAsync<TokenInvalidoException>(() => sut.ConfirmarEmail(token));
        }

        [Fact]
        public async Task ConfirmarEmail_JaConfirmado_NaoAtualizaNovamente()
        {
            var usuario = new Usuario { Id = 42, EmailConfirmado = true };
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync(usuario);
            var token = _login.GerarTokenAcao(42, "joao@learnly.com.br", "confirmacao", TimeSpan.FromHours(24));
            var sut = CriarSut();

            var retorno = await sut.ConfirmarEmail(token);

            Assert.Same(usuario, retorno);
            _usuarioRepo.Verify(r => r.Atualizar(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmarEmail_Pendente_MarcaComoConfirmado()
        {
            var usuario = new Usuario { Id = 42, EmailConfirmado = false };
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync(usuario);
            var token = _login.GerarTokenAcao(42, "joao@learnly.com.br", "confirmacao", TimeSpan.FromHours(24));
            var sut = CriarSut();

            await sut.ConfirmarEmail(token);

            Assert.True(usuario.EmailConfirmado);
            _usuarioRepo.Verify(r => r.Atualizar(usuario), Times.Once);
        }

        [Fact]
        public async Task ReenviarConfirmacao_UsuarioInexistente_NaoEnvia()
        {
            _usuarioRepo.Setup(r => r.ObterPorEmail(It.IsAny<string>())).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await sut.ReenviarConfirmacao("joao@learnly.com.br");

            _emailService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ReenviarConfirmacao_EmailJaConfirmado_NaoEnvia()
        {
            _usuarioRepo.Setup(r => r.ObterPorEmail("joao@learnly.com.br"))
                .ReturnsAsync(new Usuario { Id = 42, EmailConfirmado = true });
            var sut = CriarSut();

            await sut.ReenviarConfirmacao("joao@learnly.com.br");

            _emailService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ReenviarConfirmacao_Pendente_EnviaNovoLink()
        {
            _usuarioRepo.Setup(r => r.ObterPorEmail("joao@learnly.com.br"))
                .ReturnsAsync(new Usuario { Id = 42, Nome = "João", Email = "joao@learnly.com.br" });
            var sut = CriarSut();

            await sut.ReenviarConfirmacao("joao@learnly.com.br");

            _emailService.Verify(s => s.EnviarConfirmacaoAsync(
                "joao@learnly.com.br", "João", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SolicitarRecuperacaoSenha_UsuarioInexistente_NaoEnvia()
        {
            _usuarioRepo.Setup(r => r.ObterPorEmail(It.IsAny<string>())).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await sut.SolicitarRecuperacaoSenha("joao@learnly.com.br");

            _emailService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SolicitarRecuperacaoSenha_Existente_EnviaLinkDeRedefinicao()
        {
            _usuarioRepo.Setup(r => r.ObterPorEmail("joao@learnly.com.br"))
                .ReturnsAsync(new Usuario { Id = 42, Nome = "João", Email = "joao@learnly.com.br" });
            var sut = CriarSut();

            await sut.SolicitarRecuperacaoSenha("joao@learnly.com.br");

            _emailService.Verify(s => s.EnviarRecuperacaoSenhaAsync(
                "joao@learnly.com.br",
                "João",
                It.Is<string>(link => link.StartsWith("https://teste.learnly.com.br/redefinir-senha?token="))), Times.Once);
        }

        [Fact]
        public async Task SolicitarRecuperacaoSenha_FalhaAoEnviar_NaoPropagaExcecao()
        {
            _usuarioRepo.Setup(r => r.ObterPorEmail("joao@learnly.com.br"))
                .ReturnsAsync(new Usuario { Id = 42, Nome = "João", Email = "joao@learnly.com.br" });
            _emailService
                .Setup(s => s.EnviarRecuperacaoSenhaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("provedor fora do ar"));
            var sut = CriarSut();

            await sut.SolicitarRecuperacaoSenha("joao@learnly.com.br");
        }

        [Fact]
        public async Task RedefinirSenha_TokenDeOutroTipo_LancaTokenInvalido()
        {
            var token = _login.GerarTokenAcao(42, "joao@learnly.com.br", "confirmacao", TimeSpan.FromMinutes(30));
            var sut = CriarSut();

            await Assert.ThrowsAsync<TokenInvalidoException>(() => sut.RedefinirSenha(token, "NovaSenha@1"));
        }

        [Fact]
        public async Task RedefinirSenha_UsuarioInexistente_LancaTokenInvalido()
        {
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync((Usuario)null);
            var token = _login.GerarTokenAcao(42, "joao@learnly.com.br", "reset", TimeSpan.FromMinutes(30));
            var sut = CriarSut();

            await Assert.ThrowsAsync<TokenInvalidoException>(() => sut.RedefinirSenha(token, "NovaSenha@1"));
        }

        [Fact]
        public async Task RedefinirSenha_TokenValido_GravaHashDaNovaSenha()
        {
            var usuario = new Usuario { Id = 42, Senha = BCrypt.Net.BCrypt.HashPassword("Antiga@123") };
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync(usuario);
            var token = _login.GerarTokenAcao(42, "joao@learnly.com.br", "reset", TimeSpan.FromMinutes(30));
            var sut = CriarSut();

            await sut.RedefinirSenha(token, "NovaSenha@1");

            Assert.True(BCrypt.Net.BCrypt.Verify("NovaSenha@1", usuario.Senha));
            _usuarioRepo.Verify(r => r.Atualizar(usuario), Times.Once);
        }

        [Fact]
        public async Task DescadastrarEmails_TokenDeOutroTipo_LancaTokenInvalido()
        {
            var token = _login.GerarTokenAcao(42, "joao@learnly.com.br", "reset", TimeSpan.FromDays(30));
            var sut = CriarSut();

            await Assert.ThrowsAsync<TokenInvalidoException>(() => sut.DescadastrarEmails(token));
        }

        [Fact]
        public async Task DescadastrarEmails_UsuarioInexistente_LancaTokenInvalido()
        {
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync((Usuario)null);
            var token = _login.GerarTokenAcao(42, "joao@learnly.com.br", "unsubscribe", TimeSpan.FromDays(30));
            var sut = CriarSut();

            await Assert.ThrowsAsync<TokenInvalidoException>(() => sut.DescadastrarEmails(token));
        }

        [Fact]
        public async Task DescadastrarEmails_TokenValido_DesligaAceitaEmails()
        {
            var usuario = new Usuario { Id = 42 };
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync(usuario);
            var token = _login.GerarTokenAcao(42, "joao@learnly.com.br", "unsubscribe", TimeSpan.FromDays(30));
            var sut = CriarSut();

            await sut.DescadastrarEmails(token);

            Assert.False(usuario.AceitaEmails);
            _usuarioRepo.Verify(r => r.Atualizar(usuario), Times.Once);
        }

        [Fact]
        public async Task Atualizar_UsuarioInexistente_LancaUsuarioNaoEncontrado()
        {
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(() => sut.Atualizar(UsuarioValido()));
        }

        [Fact]
        public async Task Atualizar_NomeVazio_LancaValidationException()
        {
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync(new Usuario { Id = 42 });
            var usuario = UsuarioValido();
            usuario.Nome = "";
            var sut = CriarSut();

            await Assert.ThrowsAsync<ValidationException>(() => sut.Atualizar(usuario));
        }

        [Fact]
        public async Task Atualizar_NovoEmailJaEmUso_LancaEmailJaCadastrado()
        {
            _usuarioRepo.Setup(r => r.Obter(42, true))
                .ReturnsAsync(new Usuario { Id = 42, Email = "antigo@learnly.com.br" });
            _usuarioRepo.Setup(r => r.EmailEmUso("joao@learnly.com.br", 42)).ReturnsAsync(true);
            var sut = CriarSut();

            await Assert.ThrowsAsync<EmailJaCadastradoException>(() => sut.Atualizar(UsuarioValido()));
        }

        [Fact]
        public async Task Atualizar_MesmoEmail_NaoChecaDisponibilidade()
        {
            var existente = new Usuario { Id = 42, Nome = "Antigo", Email = "joao@learnly.com.br" };
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync(existente);
            var sut = CriarSut();

            await sut.Atualizar(UsuarioValido());

            Assert.Equal("João", existente.Nome);
            _usuarioRepo.Verify(r => r.EmailEmUso(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            _usuarioRepo.Verify(r => r.Atualizar(existente), Times.Once);
        }

        [Fact]
        public async Task AtualizarFoto_UsuarioInexistente_LancaUsuarioNaoEncontrado()
        {
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(() => sut.AtualizarFoto(42, "foto.png"));
        }

        [Fact]
        public async Task AtualizarFoto_UsuarioExistente_GravaNovaFoto()
        {
            var usuario = new Usuario { Id = 42 };
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync(usuario);
            var sut = CriarSut();

            await sut.AtualizarFoto(42, "foto.png");

            Assert.Equal("foto.png", usuario.Foto);
            _usuarioRepo.Verify(r => r.Atualizar(usuario), Times.Once);
        }

        [Fact]
        public async Task AtualizarSenha_UsuarioInexistente_LancaUsuarioNaoEncontrado()
        {
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(
                () => sut.AtualizarSenha(42, "Antiga@123", "Nova@123"));
        }

        [Fact]
        public async Task AtualizarSenha_SenhaAtualIncorreta_LancaSenhaInvalida()
        {
            _usuarioRepo.Setup(r => r.Obter(42, true))
                .ReturnsAsync(new Usuario { Id = 42, Senha = BCrypt.Net.BCrypt.HashPassword("Antiga@123") });
            var sut = CriarSut();

            await Assert.ThrowsAsync<SenhaInvalidaException>(
                () => sut.AtualizarSenha(42, "Errada@123", "Nova@123"));
        }

        [Fact]
        public async Task AtualizarSenha_SenhaAtualCorreta_GravaNovoHash()
        {
            var usuario = new Usuario { Id = 42, Senha = BCrypt.Net.BCrypt.HashPassword("Antiga@123") };
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync(usuario);
            var sut = CriarSut();

            await sut.AtualizarSenha(42, "Antiga@123", "Nova@123");

            Assert.True(BCrypt.Net.BCrypt.Verify("Nova@123", usuario.Senha));
            _usuarioRepo.Verify(r => r.Atualizar(usuario), Times.Once);
        }

        [Fact]
        public async Task Obter_UsuarioInexistente_LancaUsuarioNaoEncontrado()
        {
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(() => sut.Obter(42));
        }

        [Fact]
        public async Task Obter_UsuarioExistente_Retorna()
        {
            var usuario = new Usuario { Id = 42 };
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync(usuario);
            var sut = CriarSut();

            Assert.Same(usuario, await sut.Obter(42));
        }

        [Fact]
        public async Task ObterPorEmail_Inexistente_LancaUsuarioNaoEncontrado()
        {
            _usuarioRepo.Setup(r => r.ObterPorEmail(It.IsAny<string>())).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(
                () => sut.ObterPorEmail("joao@learnly.com.br"));
        }

        [Fact]
        public async Task ObterPorEmail_Existente_Retorna()
        {
            var usuario = new Usuario { Id = 42 };
            _usuarioRepo.Setup(r => r.ObterPorEmail("joao@learnly.com.br")).ReturnsAsync(usuario);
            var sut = CriarSut();

            Assert.Same(usuario, await sut.ObterPorEmail("joao@learnly.com.br"));
        }

        [Fact]
        public async Task ObterPorNome_Inexistente_LancaUsuarioNaoEncontrado()
        {
            _usuarioRepo.Setup(r => r.ObterPorNome(It.IsAny<string>())).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(() => sut.ObterPorNome("João"));
        }

        [Fact]
        public async Task ObterPorNome_Existente_Retorna()
        {
            var usuario = new Usuario { Id = 42 };
            _usuarioRepo.Setup(r => r.ObterPorNome("João")).ReturnsAsync(usuario);
            var sut = CriarSut();

            Assert.Same(usuario, await sut.ObterPorNome("João"));
        }

        [Fact]
        public async Task Desativar_UsuarioInexistente_LancaUsuarioNaoEncontrado()
        {
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(() => sut.Desativar(42));
        }

        [Fact]
        public async Task Desativar_UsuarioAtivo_DesligaStatusConta()
        {
            var usuario = new Usuario { Id = 42 };
            _usuarioRepo.Setup(r => r.Obter(42, true)).ReturnsAsync(usuario);
            var sut = CriarSut();

            await sut.Desativar(42);

            Assert.False(usuario.StatusConta);
            _usuarioRepo.Verify(r => r.Atualizar(usuario), Times.Once);
        }

        [Fact]
        public async Task Reativar_UsuarioInexistente_LancaUsuarioNaoEncontrado()
        {
            _usuarioRepo.Setup(r => r.Obter(42, false)).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(() => sut.Reativar(42));
        }

        [Fact]
        public async Task Reativar_UsuarioInativo_LigaStatusConta()
        {
            var usuario = new Usuario { Id = 42 };
            usuario.Desativar();
            _usuarioRepo.Setup(r => r.Obter(42, false)).ReturnsAsync(usuario);
            var sut = CriarSut();

            await sut.Reativar(42);

            Assert.True(usuario.StatusConta);
            _usuarioRepo.Verify(r => r.Atualizar(usuario), Times.Once);
        }

        [Fact]
        public async Task Listar_DelegaParaRepositorio()
        {
            var usuarios = new List<Usuario> { new() { Id = 1 } };
            _usuarioRepo.Setup(r => r.Listar(true)).ReturnsAsync(usuarios);
            var sut = CriarSut();

            Assert.Same(usuarios, await sut.Listar(true));
        }

        [Fact]
        public async Task Aquecer_DelegaParaRepositorio()
        {
            var sut = CriarSut();

            await sut.Aquecer();

            _usuarioRepo.Verify(r => r.Aquecer(), Times.Once);
        }
    }
}
