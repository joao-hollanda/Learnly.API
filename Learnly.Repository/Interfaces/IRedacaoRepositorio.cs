using Learnly.Domain.Entities.Redacoes;

namespace Learnly.Repository.Interfaces
{
    public interface IRedacaoRepositorio
    {
        Task<int> Criar(Redacao redacao);
        Task<List<Redacao>> Listar(int usuarioId);
        Task<Redacao?> Obter(int redacaoId);
    }
}
