using Learnly.Domain.Entities.Social;

namespace Learnly.Repository.Interfaces
{
    public interface IGrupoRepositorio
    {
        Task Criar(Grupo grupo);
        Task<Grupo?> Obter(int grupoId);
        Task<Grupo?> ObterPorChave(string chave);
        Task<bool> ChaveExiste(string chave);
        Task<List<Grupo>> ListarPorUsuario(int usuarioId);
        Task<GrupoMembro?> ObterMembro(int grupoId, int usuarioId);
        Task AdicionarMembro(GrupoMembro membro);
        Task RemoverMembro(GrupoMembro membro);
        Task AtualizarMembro(GrupoMembro membro);
        Task<bool> EhMembro(int grupoId, int usuarioId);
    }
}
