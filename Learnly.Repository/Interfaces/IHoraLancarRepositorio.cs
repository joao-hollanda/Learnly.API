using Learnly.Domain.Entities;

namespace Learnly.Repository.Interfaces
{
    public interface IHoraLancadaRepositorio
    {
        Task LancarHorasAsync(HoraLancada horaLancada);
        Task<HoraLancada?> ObterPorUsuarioEDataAsync(int usuarioId, DateTime data);
        Task<int> SomarHorasPeriodoAsync(int usuarioId, DateTime inicio, DateTime fim);
        Task<List<HoraLancada>> ListarPeriodoAsync(int usuarioId, DateTime inicio, DateTime fim);
    }
}
