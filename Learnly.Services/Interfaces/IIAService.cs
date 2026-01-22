using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using consumindoIA.Domain;

namespace Learnly.Services.Interfaces
{
    public interface IIAService
    {
        Task<string> GerarFeedbackAsync(Simulado simulado);
        Task<string> Chatbot(List<Message> mensagens);
    }
}