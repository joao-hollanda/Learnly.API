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

        public async Task<List<Materia>> Listar()
        {
            return await _context.Materias
                .OrderBy(m => m.Nome)
                .ToListAsync();
        }

        public async Task<Materia?> Obter(int materiaId)
        {
            return await _context.Materias
                .FirstOrDefaultAsync(m => m.MateriaId == materiaId);
        }
    }
}
