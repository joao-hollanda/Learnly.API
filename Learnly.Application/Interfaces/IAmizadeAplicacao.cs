using Learnly.Application.DTOs;

namespace Learnly.Application.Interfaces
{
    public interface IAmizadeAplicacao
    {
        Task<SolicitacaoAmizadeDto> EnviarSolicitacao(int solicitanteId, string emailOuNome);
        Task Aceitar(int amizadeId, int usuarioId);
        Task Recusar(int amizadeId, int usuarioId);
        Task Remover(int amizadeId, int usuarioId);
        Task<List<AmigoDto>> ListarAmigos(int usuarioId);
        Task<List<SolicitacaoAmizadeDto>> ListarPendentesRecebidas(int usuarioId);
        Task<List<SolicitacaoAmizadeDto>> ListarPendentesEnviadas(int usuarioId);
    }
}
