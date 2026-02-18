using Learnly.Domain.Entities;

namespace Learnly.Application.Interfaces
{
    public interface IUsuarioAplicacao
    {
        Task<int> Criar(Usuario usuarioDTO);
        Task<IEnumerable<Usuario>> Listar(bool ativo);
        Task<Usuario> Obter(int usuarioId);
        Task<Usuario> ObterPorEmail(string email);
        // Task AlterarSenha(int usuarioId, string senhaAntiga, string novaSenha);
        Task Atualizar(Usuario usuarioDTO);
        Task Desativar(int usuarioId);
        Task Reativar(int usuarioId);
    }
}