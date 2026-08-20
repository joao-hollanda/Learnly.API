using System.Net;
using Learnly.Services.BuscaService;
using Xunit;

namespace Learnly.Tests
{
    public class BuscaMaterialServiceTests
    {
        private readonly ManipuladorHttpFalso _http = new();

        private BuscaMaterialService CriarSut(string youTubeKey = "chave-teste") => new(
            _http.CriarCliente(),
            new BuscaOptions { YouTubeKey = youTubeKey });

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ResolverUrlAsync_TermoVazio_RetornaNulo(string termo)
        {
            Assert.Null(await CriarSut().ResolverUrlAsync(termo));
        }

        [Fact]
        public async Task ResolverUrlAsync_VideoEncontrado_RetornaLinkDoVideo()
        {
            _http.Responder("{\"items\":[{\"id\":{\"videoId\":\"abc123\"}}]}");
            var sut = CriarSut();

            var url = await sut.ResolverUrlAsync("função afim ferretto");

            Assert.Equal("https://www.youtube.com/watch?v=abc123", url);
            Assert.Contains("key=chave-teste", _http.Requisicoes[0].RequestUri.AbsoluteUri);
            Assert.Contains("fun%C3%A7%C3%A3o%20afim%20ferretto", _http.Requisicoes[0].RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task ResolverUrlAsync_SemChaveDoYouTube_CaiNaBuscaGenerica()
        {
            var sut = CriarSut(null);

            var url = await sut.ResolverUrlAsync("função afim");

            Assert.Equal("https://www.youtube.com/results?search_query=fun%C3%A7%C3%A3o%20afim", url);
            Assert.Empty(_http.Requisicoes);
        }

        [Fact]
        public async Task ResolverUrlAsync_BuscaSemResultados_CaiNaBuscaGenerica()
        {
            _http.Responder("{\"items\":[]}");
            var sut = CriarSut();

            var url = await sut.ResolverUrlAsync("tema inexistente");

            Assert.StartsWith("https://www.youtube.com/results?search_query=", url);
        }

        [Fact]
        public async Task ResolverUrlAsync_ApiIndisponivel_CaiNaBuscaGenerica()
        {
            _http.Responder("quota exceeded", HttpStatusCode.Forbidden);
            var sut = CriarSut();

            var url = await sut.ResolverUrlAsync("geometria plana");

            Assert.Equal("https://www.youtube.com/results?search_query=geometria%20plana", url);
        }
    }
}
