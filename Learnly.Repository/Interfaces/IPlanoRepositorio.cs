using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;

namespace Learnly.Repository.Interfaces
{
    public interface IPlanoRepositorio
    {
        Task Criar(PlanoEstudo plano);
        Task<PlanoEstudo?> Obter(int planoId);
        Task<List<PlanoEstudo>> Listar5(int usuarioId);
        Task Atualizar(List<PlanoEstudo> planos);
        Task<PlanoMateria?> ObterPlanoMateriaPorId(int planoMateriaId);
        Task Salvar();
        Task<ResumoGeralDto> GerarResumoGeral(int usuarioId);
        Task<PlanoEstudo> ObterPlanoPorId(int planoId);
    }
}