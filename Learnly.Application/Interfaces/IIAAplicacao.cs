using consumindoIA.Domain;
using Learnly.Application.DTOs;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Simulados;

namespace Learnly.Application.Interfaces
{
    public interface IIAAplicacao
    {
        Task<string> GerarFeedbackAsync(int simuladoId, int usuarioId);

        Task<PlanoEstudo> GerarPlanoAsync(int usuarioId, CriarPlanoIADTO dto);

        Task<object> ChatbotAsync(List<Message> mensagens, int usuarioId);

        Task<List<ExplicacaoQuestao>> GerarExplicacoesAsync(int simuladoId, int usuarioId);
    }
}