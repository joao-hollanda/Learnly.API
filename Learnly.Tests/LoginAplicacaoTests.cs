using System.IdentityModel.Tokens.Jwt;
using Learnly.Application.Applications;
using Learnly.Domain.Entities;
using Learnly.Domain.Exceptions.Autenticacao;
using Microsoft.Extensions.Configuration;

namespace Learnly.Tests
{
    public class LoginAplicacaoTests
    {
        private static LoginAplicacao CriarSut(string? secret = null) => new(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["jwt:secretKey"] = secret ?? "chave-de-teste-suficientemente-longa-para-hmac-sha256-0123456789",
                    ["jwt:issuer"] = "http://teste",
                    ["jwt:audience"] = "http://teste"
                })
                .Build());

        [Fact]
        public void ValidarLogin_UsuarioNulo_LancaCredenciaisInvalidas()
        {
            var sut = CriarSut();

            Assert.Throws<CredenciaisInvalidasException>(() => sut.ValidarLogin(null, "Senha@123"));
        }

        [Fact]
        public void ValidarLogin_SenhaVazia_LancaCredenciaisInvalidas()
        {
            var sut = CriarSut();
            var usuario = new Usuario { Senha = BCrypt.Net.BCrypt.HashPassword("Senha@123") };

            Assert.Throws<CredenciaisInvalidasException>(() => sut.ValidarLogin(usuario, ""));
        }

        [Fact]
        public void ValidarLogin_SenhaErrada_RetornaFalse()
        {
            var sut = CriarSut();
            var usuario = new Usuario { Senha = BCrypt.Net.BCrypt.HashPassword("Senha@123") };

            Assert.False(sut.ValidarLogin(usuario, "Errada@123"));
        }

        [Fact]
        public void ValidarLogin_SenhaCorreta_RetornaTrue()
        {
            var sut = CriarSut();
            var usuario = new Usuario { Senha = BCrypt.Net.BCrypt.HashPassword("Senha@123") };

            Assert.True(sut.ValidarLogin(usuario, "Senha@123"));
        }

        [Fact]
        public void GenerateToken_Padrao_GeraAccessTokenComClaims()
        {
            var sut = CriarSut();

            var token = new JwtSecurityTokenHandler().ReadJwtToken(
                sut.GenerateToken(42, "a@b.com", "Aluno"));

            Assert.Equal("42", token.Claims.First(c => c.Type == "id").Value);
            Assert.Equal("a@b.com", token.Claims.First(c => c.Type == "email").Value);
            Assert.Equal("Aluno", token.Claims.First(c => c.Type == "nome").Value);
            Assert.Equal("access", token.Claims.First(c => c.Type == "tipo").Value);
        }

        [Fact]
        public void GenerateToken_ComFlagRefresh_GeraTipoRefresh()
        {
            var sut = CriarSut();

            var token = new JwtSecurityTokenHandler().ReadJwtToken(
                sut.GenerateToken(42, "a@b.com", "Aluno", TimeSpan.FromDays(30), refreshToken: true));

            Assert.Equal("refresh", token.Claims.First(c => c.Type == "tipo").Value);
        }

        [Fact]
        public void ValidarToken_TokenValido_RetornaPrincipal()
        {
            var sut = CriarSut();
            var token = sut.GenerateToken(42, "a@b.com", "Aluno", TimeSpan.FromMinutes(10), refreshToken: true);

            var principal = sut.ValidarToken(token);

            Assert.NotNull(principal);
            Assert.Equal("42", principal!.FindFirst("id")?.Value);
            Assert.Equal("refresh", principal.FindFirst("tipo")?.Value);
        }

        [Fact]
        public void ValidarToken_TokenExpirado_RetornaNull()
        {
            var sut = CriarSut();
            // além do ClockSkew padrão de 5 minutos
            var token = sut.GenerateToken(42, "a@b.com", "Aluno", TimeSpan.FromMinutes(-10), refreshToken: true);

            Assert.Null(sut.ValidarToken(token));
        }

        [Fact]
        public void ValidarToken_AssinaturaDeOutroSegredo_RetornaNull()
        {
            var outroSut = CriarSut("outro-segredo-completamente-diferente-para-invalidar-assinatura");
            var token = outroSut.GenerateToken(42, "a@b.com", "Aluno", TimeSpan.FromMinutes(10), refreshToken: true);

            var sut = CriarSut();

            Assert.Null(sut.ValidarToken(token));
        }
    }
}
