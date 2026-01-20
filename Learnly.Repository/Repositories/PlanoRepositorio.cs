using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Learnly.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Repository
{
    public class PlanoRepositorio : BaseRepositorio, IPlanoRepositorio
    {
        public PlanoRepositorio(LearnlyContexto contexto) : base(contexto)
        {
        }

        public async Task Criar(PlanoEstudo plano)
        {
            await _contexto.PlanosEstudo.AddAsync(plano);
            await _contexto.SaveChangesAsync();
        }

        public async Task<List<PlanoEstudo>> Listar5(int usuarioId)
        {
            return await _contexto.PlanosEstudo
                .Where(p => p.UsuarioId == usuarioId && p.Ativo == true)
                .Include(p => p.PlanoMaterias)
                    .ThenInclude(pm => pm.Materia)
                .OrderByDescending(p => p.PlanoId)
                .Take(5)
                .ToListAsync();
        }

        public async Task<PlanoEstudo?> Obter(int planoId)
        {
            return await _contexto.PlanosEstudo
                .Include(p => p.PlanoMaterias)
                    .ThenInclude(pm => pm.Materia)
                .FirstOrDefaultAsync(p => p.PlanoId == planoId);
        }

        public async Task Atualizar(List<PlanoEstudo> planos)
        {
            _contexto.PlanosEstudo.UpdateRange(planos);
            await _contexto.SaveChangesAsync();
        }

        public async Task<PlanoEstudo> ObterPlanoPorId(int planoId)
        {
            return await _contexto.PlanosEstudo.FirstOrDefaultAsync(p => p.PlanoId == planoId);
        }
        public async Task<PlanoMateria?> ObterPlanoMateriaPorId(int planoMateriaId)
        {
            return await _contexto.PlanoMateria
                .FirstOrDefaultAsync(pm => pm.PlanoMateriaId == planoMateriaId);
        }

        public async Task Salvar()
        {
            await _contexto.SaveChangesAsync();
        }

        public async Task<ResumoGeralUsuarioDto> GerarResumoGeral(int usuarioId)
        {
            return await _contexto.ResumoGeral
                .FromSqlRaw(
                    "EXEC sp_GerarResumoGeralUsuario @UsuarioId = @usuarioId",
                    new SqlParameter("@usuarioId", usuarioId)
                )
                .AsNoTracking()
                .FirstAsync();
        }



        private static int CalcularSemanas(DateTime inicio, DateTime fim)
        {
            if (fim.Date < inicio.Date)
                throw new ArgumentException("Data final não pode ser menor que a inicial");

            var dias = (fim.Date - inicio.Date).TotalDays;
            return Math.Max(1, (int)Math.Ceiling(dias / 7));
        }


    }
}