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
            var existente = await ObterPorUsuarioEDataAsync(
                horaLancada.UsuarioId,
                horaLancada.Data.Date
            );

            if (existente == null)
            {
                await _contexto.HorasLancadas.AddAsync(horaLancada);
            }
            else
            {
                existente.QuantdadeHoras += horaLancada.QuantdadeHoras;
                _contexto.HorasLancadas.Update(existente);
            }

            await _contexto.SaveChangesAsync();
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
            return await _contexto.HorasLancadas
                .Where(h =>
                    h.UsuarioId == usuarioId &&
                    h.Data.Date >= inicio.Date &&
                    h.Data.Date <= fim.Date
                )
                .SumAsync(h => h.QuantdadeHoras);
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
