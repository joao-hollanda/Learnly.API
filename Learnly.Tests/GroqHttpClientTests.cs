using System.Net;
using Learnly.Services.IAService;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Learnly.Tests
{
    public class GroqHttpClientTests
    {
        private readonly ManipuladorHttpFalso _http = new();

        private GroqHttpClient CriarSut() => new(_http.CriarCliente());

        [Fact]
        public async Task EnviarAsync_RespostaValida_RetornaConteudoDaMensagem()
        {
            _http.ResponderChat("resposta do modelo");
            var sut = CriarSut();

            var conteudo = await sut.EnviarAsync(new { model = "llama-3.3-70b-versatile" });

            Assert.Equal("resposta do modelo", conteudo);
            var requisicao = Assert.Single(_http.Requisicoes);
            Assert.Equal("https://api.groq.com/openai/v1/chat/completions", requisicao.RequestUri.ToString());
            Assert.Equal("llama-3.3-70b-versatile", JObject.Parse(_http.CorposEnviados[0])["model"]);
        }

        [Fact]
        public async Task EnviarAsync_SemChoices_RetornaNulo()
        {
            _http.Responder("{\"choices\":null}");
            var sut = CriarSut();

            Assert.Null(await sut.EnviarAsync(new { }));
        }

        [Fact]
        public async Task EnviarCompletaAsync_RemoveBlocoDeRaciocinio()
        {
            _http.ResponderChat("<think>\nrascunho interno\n</think>\n\nResposta final ao aluno");
            var sut = CriarSut();

            var mensagem = await sut.EnviarCompletaAsync(new { });

            Assert.Equal("Resposta final ao aluno", mensagem.content);
        }

        [Fact]
        public async Task EnviarCompletaAsync_SemConteudo_MantemMensagem()
        {
            _http.Responder("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"tool_calls\":[]}}]}");
            var sut = CriarSut();

            var mensagem = await sut.EnviarCompletaAsync(new { });

            Assert.Null(mensagem.content);
            Assert.Equal("assistant", mensagem.role);
        }

        [Fact]
        public async Task Post_StatusDeErro_LancaComStatusECorpo()
        {
            _http.Responder("{\"error\":\"chave inválida\"}", HttpStatusCode.Unauthorized);
            var sut = CriarSut();

            var ex = await Assert.ThrowsAsync<Exception>(() => sut.EnviarAsync(new { }));

            Assert.Contains("401", ex.Message);
            Assert.Contains("chave inválida", ex.Message);
        }

        [Fact]
        public async Task Post_RateLimit_AguardaOTempoSugeridoETentaDeNovo()
        {
            _http.Responder(
                "{\"error\":{\"message\":\"Rate limit reached, please try again in 0.5s\"}}",
                HttpStatusCode.TooManyRequests);
            _http.ResponderChat("consegui responder");
            var sut = CriarSut();

            Assert.Equal("consegui responder", await sut.EnviarAsync(new { }));
            Assert.Equal(2, _http.Requisicoes.Count);
        }

        [Fact]
        public async Task Post_RateLimitPersistente_LancaMensagemDeSobrecarga()
        {
            for (int i = 0; i < 3; i++)
                _http.Responder(
                    "{\"error\":{\"message\":\"Rate limit reached, please try again in 0.2s\"}}",
                    HttpStatusCode.TooManyRequests);
            var sut = CriarSut();

            var ex = await Assert.ThrowsAsync<Exception>(() => sut.EnviarAsync(new { }));

            Assert.Contains("sobrecarregado", ex.Message);
            Assert.Equal(3, _http.Requisicoes.Count);
        }
    }
}
