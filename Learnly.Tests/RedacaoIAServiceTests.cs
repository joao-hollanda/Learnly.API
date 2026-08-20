using System.Net;
using Learnly.Services.IAService;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Learnly.Tests
{
    public class RedacaoIAServiceTests
    {
        private readonly ManipuladorHttpFalso _http = new();

        private RedacaoIAService CriarSut() => new(new MistralHttpClient(_http.CriarCliente()));

        [Fact]
        public async Task TranscreverImagemAsync_EnviaImagemComoDataUri()
        {
            _http.ResponderChat("  Texto transcrito da redação  ");
            var sut = CriarSut();

            var texto = await sut.TranscreverImagemAsync(new byte[] { 1, 2, 3 }, "image/png");

            Assert.Equal("Texto transcrito da redação", texto);
            var corpo = JObject.Parse(_http.CorposEnviados[0]);
            Assert.Equal("mistral-small-latest", corpo["model"]);
            Assert.Equal($"data:image/png;base64,{Convert.ToBase64String(new byte[] { 1, 2, 3 })}",
                corpo["messages"][0]["content"][1]["image_url"]);
        }

        [Fact]
        public async Task TranscreverImagemAsync_RespostaNula_LancaExcecao()
        {
            _http.Responder("{\"choices\":null}");
            var sut = CriarSut();

            var ex = await Assert.ThrowsAsync<Exception>(
                () => sut.TranscreverImagemAsync(new byte[] { 1 }, "image/png"));

            Assert.Contains("transcrever", ex.Message);
        }

        [Fact]
        public async Task GerarTemaAsync_RemoveAspasEEspacos()
        {
            _http.ResponderChat("  \"Desafios da saúde mental na juventude brasileira\"  ");
            var sut = CriarSut();

            var tema = await sut.GerarTemaAsync();

            Assert.Equal("Desafios da saúde mental na juventude brasileira", tema);
            Assert.Contains("Eixo temático sorteado:", JObject.Parse(_http.CorposEnviados[0])["messages"][1]["content"].ToString());
        }

        [Fact]
        public async Task GerarTemaAsync_RespostaNula_LancaExcecao()
        {
            _http.Responder("{\"choices\":null}");
            var sut = CriarSut();

            await Assert.ThrowsAsync<Exception>(() => sut.GerarTemaAsync());
        }

        [Fact]
        public async Task CorrigirAsync_RemoveCercaDeMarkdownEDesserializa()
        {
            _http.ResponderChat("```json\n{\"competencias\":[{\"numero\":1,\"nota\":160,\"comentario\":\"boa norma culta\"}],\"comentarioGeral\":\"bom texto\"}\n```");
            var sut = CriarSut();

            var correcao = await sut.CorrigirAsync("Tema", "Texto do aluno");

            Assert.Equal("bom texto", correcao.ComentarioGeral);
            var competencia = Assert.Single(correcao.Competencias);
            Assert.Equal(1, competencia.Numero);
            Assert.Equal(160, competencia.Nota);

            var corpo = JObject.Parse(_http.CorposEnviados[0]);
            Assert.Equal("mistral-medium-latest", corpo["model"]);
            Assert.Equal("json_object", corpo["response_format"]["type"]);
            Assert.Contains("Texto do aluno", corpo["messages"][1]["content"].ToString());
        }

        [Fact]
        public async Task CorrigirAsync_RespostaNula_LancaExcecao()
        {
            _http.Responder("{\"choices\":null}");
            var sut = CriarSut();

            await Assert.ThrowsAsync<Exception>(() => sut.CorrigirAsync("Tema", "Texto"));
        }

        [Fact]
        public async Task MistralIndisponivel_PropagaStatusECorpo()
        {
            _http.Responder("{\"message\":\"invalid api key\"}", HttpStatusCode.Unauthorized);
            var sut = CriarSut();

            var ex = await Assert.ThrowsAsync<Exception>(() => sut.GerarTemaAsync());

            Assert.Contains("Mistral retornou 401", ex.Message);
        }
    }
}
