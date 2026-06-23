using Learnly.Domain.Entities.Redacoes;

namespace Learnly.Application.Interfaces
{
    public interface IRedacaoIAService
    {
        Task<string> TranscreverImagemAsync(byte[] imagem, string mimeType);
        Task<CorrecaoRedacao> CorrigirAsync(string tema, string texto);
        Task<string> GerarTemaAsync();
    }
}
