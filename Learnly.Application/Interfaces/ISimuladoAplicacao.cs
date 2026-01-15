using Learnly.Domain.Entities.Simulados;

namespace Learnly.Application.Interfaces
{
    public interface ISimuladoAplicacao
    {
        Task<int> GerarSimulado(Simulado simulado, List<string> disciplinas, int totalQuestoes = 25);
        Task<Simulado> ResponderSimulado(Simulado simulado);
        Task<Simulado> Obter(int id);
        Task<List<Simulado>> Listar5(int usuarioId);
    }
}