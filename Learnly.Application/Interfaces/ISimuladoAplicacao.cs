using Learnly.Application.DTOs;
using Learnly.Domain.Entities.Simulados;

namespace Learnly.Application.Interfaces
{
    public interface ISimuladoAplicacao
    {
        Task<int> GerarSimulado(Simulado simulado, List<string> disciplinas, int totalQuestoes = 25);
        Task<Simulado> ResponderSimulado(int simuladoId, List<RespostaSimulado> respostas, int usuarioId);
        Task<Simulado> Obter(int simuladoId, int usuarioId);
        Task<List<Simulado>> Listar(int usuarioId, int quantidade = 5);
        Task<List<SimuladoResumoDto>> ListarResumo(int usuarioId, int quantidade = 9);
        Task<int> Contar(int usuarioId);
    }
}
