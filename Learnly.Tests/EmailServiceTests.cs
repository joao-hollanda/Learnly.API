using System.Net;
using Learnly.Services.Email;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Learnly.Tests
{
    public class EmailServiceTests
    {
        private readonly ManipuladorHttpFalso _http = new();

        private EmailService CriarSut(string apiKey = "re_teste") => new(
            _http.CriarCliente(),
            new EmailOptions { ApiKey = apiKey, From = "Learnly <nao-responda@learnly.com.br>" });

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SemApiKey_NaoChamaOProvedor(string apiKey)
        {
            var sut = CriarSut(apiKey);

            await sut.EnviarConfirmacaoAsync("joao@learnly.com.br", "João", "https://learnly.com.br/confirmar");

            Assert.Empty(_http.Requisicoes);
        }

        [Fact]
        public async Task EnviarConfirmacaoAsync_MontaRequisicaoParaOResend()
        {
            _http.Responder("{\"id\":\"abc\"}");
            var sut = CriarSut();

            await sut.EnviarConfirmacaoAsync("joao@learnly.com.br", "João", "https://learnly.com.br/confirmar");

            var requisicao = Assert.Single(_http.Requisicoes);
            Assert.Equal(HttpMethod.Post, requisicao.Method);
            Assert.Equal("https://api.resend.com/emails", requisicao.RequestUri.ToString());
            Assert.Equal("Bearer", requisicao.Headers.Authorization.Scheme);
            Assert.Equal("re_teste", requisicao.Headers.Authorization.Parameter);

            var corpo = JObject.Parse(_http.CorposEnviados[0]);
            Assert.Equal("Learnly <nao-responda@learnly.com.br>", corpo["from"]);
            Assert.Equal("joao@learnly.com.br", corpo["to"][0]);
            Assert.Equal("Confirme seu e-mail · Learnly", corpo["subject"]);
            var html = corpo["html"].ToString();
            Assert.Contains("Olá, João!", html);
            Assert.Contains("https://learnly.com.br/confirmar", html);
            Assert.Contains("Confirmar e-mail", html);
        }

        [Fact]
        public async Task EnviarRecuperacaoSenhaAsync_UsaAssuntoEBotaoProprios()
        {
            _http.Responder("{\"id\":\"abc\"}");
            var sut = CriarSut();

            await sut.EnviarRecuperacaoSenhaAsync("joao@learnly.com.br", "João", "https://learnly.com.br/redefinir");

            var corpo = JObject.Parse(_http.CorposEnviados[0]);
            Assert.Equal("Redefinição de senha · Learnly", corpo["subject"]);
            Assert.Contains("Redefinir senha", corpo["html"].ToString());
        }

        [Fact]
        public async Task EnviarLembreteSequenciaAsync_IncluiLinkDeDescadastro()
        {
            _http.Responder("{\"id\":\"abc\"}");
            var sut = CriarSut();

            await sut.EnviarLembreteSequenciaAsync(
                "joao@learnly.com.br", "João", 12,
                "https://learnly.com.br/planos",
                "https://learnly.com.br/descadastrar?token=abc");

            var corpo = JObject.Parse(_http.CorposEnviados[0]);
            Assert.Equal("Sua sequência de 12 dias termina hoje · Learnly", corpo["subject"]);
            var html = corpo["html"].ToString();
            Assert.Contains("<strong>12 dias</strong>", html);
            Assert.Contains("https://learnly.com.br/descadastrar?token=abc", html);
            Assert.Contains("Não quero mais receber estes lembretes", html);
        }

        [Fact]
        public async Task ProvedorRetornaErro_LancaExcecaoComStatusEDetalhe()
        {
            _http.Responder("{\"message\":\"domain not verified\"}", HttpStatusCode.UnprocessableEntity);
            var sut = CriarSut();

            var ex = await Assert.ThrowsAsync<Exception>(
                () => sut.EnviarConfirmacaoAsync("joao@learnly.com.br", "João", "https://learnly.com.br/confirmar"));

            Assert.Contains("422", ex.Message);
            Assert.Contains("domain not verified", ex.Message);
        }
    }
}
