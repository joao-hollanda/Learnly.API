using Learnly.Domain.Entities.Planos;
using Learnly.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Repository.Repositories
{
    public class MateriaRepositorio : IMateriaRepositorio
    {
        private readonly LearnlyContexto _context;

        public MateriaRepositorio(LearnlyContexto context)
        {
            _context = context;
        }

        public async Task<List<Materia>> Listar(bool geradaPorIa)
        {
            return await _context.Materias
                .OrderBy(m => m.MateriaId)
                .Where(m => m.GeradaPorIA == geradaPorIa)
                .ToListAsync();
        }

        public async Task<Materia?> Obter(int materiaId)
        {
            return await _context.Materias
                .FirstOrDefaultAsync(m => m.MateriaId == materiaId);
        }

        public async Task<Materia?> ObterPorNome(string nome)
        {
            return await _context.Materias
                .FirstOrDefaultAsync(m =>
                    EF.Functions.ILike(m.Nome, nome));
        }
    }
}
