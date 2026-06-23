using Learnly.Domain.Entities;

namespace Learnly.Repository.Interfaces
{
    public interface IUsuarioRepositorio
    {
        Task<int> Criar(Usuario usuario);
        Task Atualizar(Usuario usuario);
        Task<Usuario> Obter(int usuarioId, bool ativo);
        Task<Usuario> ObterPorNome(string nome);
        Task<Usuario> ObterPorEmail(string email);
        Task<IEnumerable<Usuario>> Listar(bool ativo);
        Task Aquecer();
    }
}