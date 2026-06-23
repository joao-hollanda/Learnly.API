using Learnly.Domain.Entities.Redacoes;

namespace Learnly.Application.Interfaces
{
    public interface IRedacaoAplicacao
    {
        Task<string> Transcrever(byte[] imagem, string mimeType);
        Task<string> GerarTema();
        Task<Redacao> Corrigir(int usuarioId, string tema, string texto);
        Task<List<Redacao>> Listar(int usuarioId);
        Task<Redacao> Obter(int redacaoId, int usuarioId);
    }
}
