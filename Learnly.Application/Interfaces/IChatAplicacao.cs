using Learnly.Application.DTOs;

namespace Learnly.Application.Interfaces
{
    public interface IChatAplicacao
    {
        Task<List<ConversaResumoDto>> ListarConversas(int usuarioId);
        Task<int> ObterOuCriarConversaDireta(int usuarioId, int amigoId);
        Task<ConversaDetalheDto> ObterConversa(int conversaId, int usuarioId);
        Task<List<MensagemDto>> ListarMensagens(int conversaId, int usuarioId, int? antesDeId, int limite);
        Task<MensagemDto> EnviarMensagem(int conversaId, int remetenteId, NovaMensagemDto dto);
        Task MarcarLida(int conversaId, int usuarioId);
        Task<List<int>> ListarIdsConversas(int usuarioId);
        Task<List<int>> ListarDestinatarios(int conversaId, int excetoUsuarioId);
    }
}
