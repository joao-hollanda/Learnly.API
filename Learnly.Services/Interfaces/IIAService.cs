using consumindoIA.Domain;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Simulados;

namespace Learnly.Application.Interfaces
{
    public interface IIAService
    {
        Task<string> GerarFeedbackAsync(Simulado simulado);

        Task<PlanoEstudo> GerarPlanoAsync(PlanoEstudo plano);

        Task<Message?> EnviarMensagensAsync(ChatRequest request);

        Task<List<ExplicacaoQuestao>> GerarExplicacoesAsync(
            List<SimuladoQuestao> questoesErradas,
            Dictionary<int, RespostaSimulado> respostas);
    }
}