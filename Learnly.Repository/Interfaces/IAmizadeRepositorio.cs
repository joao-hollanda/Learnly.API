using Learnly.Domain.Entities.Social;

namespace Learnly.Repository.Interfaces
{
    public interface IAmizadeRepositorio
    {
        Task Criar(Amizade amizade);
        Task Atualizar(Amizade amizade);
        Task Remover(Amizade amizade);
        Task<Amizade?> Obter(int amizadeId);
        Task<Amizade?> ObterEntre(int usuarioA, int usuarioB);
        Task<List<Amizade>> ListarAceitas(int usuarioId);
        Task<List<Amizade>> ListarPendentesRecebidas(int usuarioId);
        Task<List<Amizade>> ListarPendentesEnviadas(int usuarioId);
    }
}
