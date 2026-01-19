using Learnly.Domain.Entities.Planos;

namespace Learnly.Application.Interfaces
{
    public interface IMateriaAplicacao
    {
        Task<List<Materia>> Listar();
    }
}
