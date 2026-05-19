using consumindoIA.Domain;
using Learnly.Domain.Entities.Simulados;

namespace Learnly.Services.Interfaces
{
    public interface IToolService
    {
        Task<string> ExecutarFerramentaAsync(string functionName, string functionArgs, int usuarioId);
        SimuladoIAResumo GerarResumoIA(Simulado simulado);
    }
}
