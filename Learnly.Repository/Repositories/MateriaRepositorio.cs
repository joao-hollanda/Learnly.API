using Learnly.Domain.Entities.Planos;
using Learnly.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Repository.Repositories
{
    public class MateriaRepositorio : BaseRepositorio, IMateriaRepositorio
    {
        public MateriaRepositorio(LearnlyContexto contexto) : base(contexto)
        {
        }

        public async Task<List<Materia>> Listar(bool geradaPorIa)
        {
            return await _contexto.Materias
                .OrderBy(m => m.MateriaId)
                .Where(m => m.GeradaPorIA == geradaPorIa)
                .ToListAsync();
        }

        public async Task<Materia?> Obter(int materiaId)
        {
            return await _contexto.Materias
                .FirstOrDefaultAsync(m => m.MateriaId == materiaId);
        }

        public async Task<Materia?> ObterPorNome(string nome)
        {
            return await _contexto.Materias
                .FirstOrDefaultAsync(m =>
                    EF.Functions.ILike(m.Nome, nome));
        }
    }
}
