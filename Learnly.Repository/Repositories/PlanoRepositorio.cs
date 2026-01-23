using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Learnly.Repository.Interfaces;
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

        public async Task<PlanoEstudo?> ObterPlanoPorId(int planoId)
        {
            return await _contexto.PlanosEstudo
                .FirstOrDefaultAsync(p => p.PlanoId == planoId);
        }

        public async Task<PlanoMateria?> ObterPlanoMateriaPorId(int planoMateriaId)
        {
            return await _contexto.PlanoMateria
                .Include(pm => pm.Plano)
                .FirstOrDefaultAsync(pm => pm.PlanoMateriaId == planoMateriaId);
        }

        public async Task Salvar()
        {
            await _contexto.SaveChangesAsync();
        }

        public async Task<ResumoGeralDto?> GerarResumoGeral(int usuarioId)
        {
            var resumo = await _contexto.PlanosEstudo
                .Where(p => p.UsuarioId == usuarioId)
                .SelectMany(p => p.PlanoMaterias)
                .GroupBy(_ => 1)
                .Select(g => new ResumoGeralDto
                {
                    HorasTotais = g.Sum(x => x.HorasTotais),
                    HorasConcluidas = g.Sum(x => x.HorasConcluidas)
                })
                .FirstOrDefaultAsync();

            return resumo;
        }

        public async Task<List<PlanoEstudo>> ListarPorUsuario(int usuarioId)
        {
            return await _contexto.PlanosEstudo
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.PlanoMaterias)
                    .ThenInclude(pm => pm.Materia)
                .OrderByDescending(p => p.PlanoId)
                .Take(5)
                .ToListAsync();
        }

        public async Task Excluir(PlanoEstudo plano)
        {
            _contexto.Remove(plano);
            await _contexto.SaveChangesAsync();
        }

        public async Task<int> ContarPorUsuario(int usuarioId)
        {
            return await _contexto.PlanosEstudo
                .CountAsync(p => p.UsuarioId == usuarioId);
        }

        public async Task<PlanoEstudo?> ObterPlanoAtivo(int usuarioId)
        {
            return await _contexto.PlanosEstudo
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.UsuarioId == usuarioId &&
                    p.Ativo
                );
        }
    }
}
