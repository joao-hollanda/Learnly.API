using Learnly.Domain.Entities.Social;

namespace Learnly.Repository.Interfaces
{
    public interface IChatRepositorio
    {
        Task CriarConversa(Conversa conversa);
        Task AtualizarConversa(Conversa conversa);
        Task<Conversa?> ObterConversa(int conversaId);
        Task<Conversa?> ObterConversaDireta(int usuarioA, int usuarioB);
        Task<Conversa?> ObterConversaDoGrupo(int grupoId);
        Task<List<Conversa>> ListarConversasDiretas(int usuarioId);

        Task AdicionarMensagem(Mensagem mensagem);
        Task<List<Mensagem>> ListarMensagens(int conversaId, int? antesDeId, int limite);
        Task<Mensagem?> ObterUltimaMensagem(int conversaId);
        Task<int> ContarNaoLidas(int conversaId, int usuarioId, DateTime? ultimaLeitura);

        Task<ConversaParticipante?> ObterParticipante(int conversaId, int usuarioId);
        Task<List<ConversaParticipante>> ListarParticipantes(int conversaId);
        Task AtualizarParticipante(ConversaParticipante participante);
    }
}
