using Learnly.Domain.Entities;

namespace Learnly.Application.Interfaces
{
    public interface IUsuarioAplicacao
    {
        Task<int> Criar(Usuario usuarioDTO);
        Task<IEnumerable<Usuario>> Listar(bool ativo);
        Task<Usuario> Obter(int usuarioId);
        Task<Usuario> ObterPorEmail(string email);
        Task AtualizarSenha(int usuarioId, string senhaAntiga, string novaSenha);
        Task AtualizarFoto(int usuarioId, string foto);
        Task Atualizar(Usuario usuarioDTO);
        Task Desativar(int usuarioId);
        Task Reativar(int usuarioId);
        Task Aquecer();
        Task<Usuario> ConfirmarEmail(string token);
        Task ReenviarConfirmacao(string email);
        Task DescadastrarEmails(string token);
        Task SolicitarRecuperacaoSenha(string email);
        Task RedefinirSenha(string token, string novaSenha);
    }
}