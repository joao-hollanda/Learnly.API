using Learnly.Application.Interfaces;
using Learnly.Domain.Entities.Planos;
using Learnly.Repository.Interfaces;

namespace Learnly.Application.Applications
{
    public class MateriaAplicacao : IMateriaAplicacao
    {
        private readonly IMateriaRepositorio _materiaRepositorio;

        public MateriaAplicacao(IMateriaRepositorio materiaRepositorio)
        {
            _materiaRepositorio = materiaRepositorio;
        }

        public async Task<List<Materia>> Listar()
        {
            return await _materiaRepositorio.Listar();
        }
    }
}
