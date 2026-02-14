using Learnly.Domain.Entities.Planos;

namespace Learnly.Repository.Interfaces
{
    public interface IMateriaRepositorio
    {
        Task<List<Materia>> Listar(bool geradaPorIa);
        Task<Materia?> Obter(int materiaId);
        Task<Materia?> ObterPorNome(string nome);
    }
}
