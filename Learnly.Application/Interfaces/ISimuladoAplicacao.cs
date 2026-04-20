using Learnly.Domain.Entities.Simulados;

namespace Learnly.Application.Interfaces
{
    public interface ISimuladoAplicacao
    {
        Task<int> GerarSimulado(Simulado simulado, List<string> disciplinas, int totalQuestoes = 25);
        Task<Simulado> ResponderSimulado(int simuladoId, List<RespostaSimulado> respostas, int usuarioId);
        Task<Simulado> Obter(int simuladoId, int usuarioId);
        Task<List<Simulado>> Listar5(int usuarioId);
        Task<int> Contar(int usuarioId);
    }
}
