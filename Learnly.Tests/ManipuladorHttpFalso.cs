using System.Net;
using System.Text;

namespace Learnly.Tests
{
    internal class ManipuladorHttpFalso : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _respostas = new();

        public List<HttpRequestMessage> Requisicoes { get; } = new();
        public List<string> CorposEnviados { get; } = new();

        public ManipuladorHttpFalso Responder(string conteudo, HttpStatusCode status = HttpStatusCode.OK)
        {
            _respostas.Enqueue(new HttpResponseMessage(status)
            {
                Content = new StringContent(conteudo, Encoding.UTF8, "application/json")
            });

            return this;
        }

        public ManipuladorHttpFalso ResponderChat(string conteudoDaMensagem)
        {
            var escapado = System.Text.Json.JsonSerializer.Serialize(conteudoDaMensagem);

            return Responder($"{{\"choices\":[{{\"message\":{{\"role\":\"assistant\",\"content\":{escapado}}}}}]}}");
        }

        public HttpClient CriarCliente() => new(this);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requisicoes.Add(request);
            CorposEnviados.Add(request.Content == null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken));

            return _respostas.Count > 0
                ? _respostas.Dequeue()
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                };
        }
    }
}
