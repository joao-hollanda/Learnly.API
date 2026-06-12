using Learnly.Application.Interfaces;
using Newtonsoft.Json.Linq;

namespace Learnly.Services.BuscaService
{
    public class BuscaMaterialService : IBuscaMaterialService
    {
        private readonly HttpClient _http;
        private readonly BuscaOptions _options;

        public BuscaMaterialService(HttpClient http, BuscaOptions options)
        {
            _http = http;
            _options = options;
        }

        public async Task<string> ResolverUrlAsync(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return null;

            try
            {
                var url = await ResolverYouTubeAsync(termo);
                if (!string.IsNullOrEmpty(url))
                    return url;
            }
            catch { }

            return $"https://www.youtube.com/results?search_query={Uri.EscapeDataString(termo)}";
        }

        private async Task<string> ResolverYouTubeAsync(string termo)
        {
            if (string.IsNullOrWhiteSpace(_options.YouTubeKey))
                return null;

            var endpoint = $"https://www.googleapis.com/youtube/v3/search?part=snippet&type=video&maxResults=1&q={Uri.EscapeDataString(termo)}&key={_options.YouTubeKey}";
            var resposta = await _http.GetStringAsync(endpoint);
            var videoId = JObject.Parse(resposta)["items"]?.FirstOrDefault()?["id"]?["videoId"]?.ToString();

            return string.IsNullOrEmpty(videoId)
                ? null
                : $"https://www.youtube.com/watch?v={videoId}";
        }
    }
}
