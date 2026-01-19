using Learnly.Domain.Entities.Planos;

namespace Learnly.Repository.Interfaces
{
    public interface IMateriaRepositorio
    {
        Task<List<Materia>> Listar();
        Task<Materia?> Obter(int materiaId);
    }
}
