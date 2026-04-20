using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using consumindoIA.Domain;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Simulados;

namespace Learnly.Services.Interfaces
{
    public interface IIAService
    {
        Task<string> GerarFeedbackAsync(Simulado simulado);
        Task<string> Chatbot(List<Message> mensagens);
        Task<PlanoEstudo> GerarPlanoIA(PlanoEstudo plano);
        Task<List<ExplicacaoQuestao>> GerarExplicacoes(
            List<SimuladoQuestao> questoesErradas,
            Dictionary<int, RespostaSimulado> respostas);
    }
}