using Learnly.Application.DTOs;

namespace Learnly.Application.Interfaces
{
    public interface IDesempenhoAplicacao
    {
        Task<DashboardDto> ObterDashboard(int usuarioId);
    }
}
