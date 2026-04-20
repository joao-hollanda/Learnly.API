using Learnly.Domain.Entities;
using Learnly.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Repository
{
    public class HoraLancadaRepositorio : BaseRepositorio, IHoraLancadaRepositorio
    {
        public HoraLancadaRepositorio(LearnlyContexto contexto) : base(contexto)
        {
        }

        public async Task LancarHorasAsync(HoraLancada horaLancada)
        {
            await _contexto.Database.ExecuteSqlRawAsync("CALL sp_lancar_horas({0}, {1}::date, {2})", 
                horaLancada.UsuarioId, 
                horaLancada.Data, 
                horaLancada.QuantdadeHoras);
        }

        public async Task<HoraLancada?> ObterPorUsuarioEDataAsync(int usuarioId, DateTime data)
        {
            return await _contexto.HorasLancadas
                .FirstOrDefaultAsync(h =>
                    h.UsuarioId == usuarioId &&
                    h.Data.Date == data.Date
                );
        }

        public async Task<int> SomarHorasPeriodoAsync(int usuarioId, DateTime inicio, DateTime fim)
        {
            return await _contexto.Database.SqlQueryRaw<int>(
                "SELECT fn_somar_horas_periodo({0}, {1}::date, {2}::date)", 
                usuarioId, 
                inicio, 
                fim).FirstOrDefaultAsync();
        }

        public async Task<List<HoraLancada>> ListarPeriodoAsync(int usuarioId, DateTime inicio, DateTime fim)
        {
            return await _contexto.HorasLancadas
                .Where(h =>
                    h.UsuarioId == usuarioId &&
                    h.Data.Date >= inicio.Date &&
                    h.Data.Date <= fim.Date
                )
                .OrderBy(h => h.Data)
                .ToListAsync();
        }
    }
}