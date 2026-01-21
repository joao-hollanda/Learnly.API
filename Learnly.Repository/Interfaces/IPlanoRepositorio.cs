using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;

namespace Learnly.Repository.Interfaces
{
    public interface IPlanoRepositorio
    {
        Task Criar(PlanoEstudo plano);
        Task<PlanoEstudo?> Obter(int planoId);
        Task Atualizar(List<PlanoEstudo> planos);
        Task<PlanoMateria?> ObterPlanoMateriaPorId(int planoMateriaId);
        Task Salvar();
        Task<ResumoGeralUsuarioDto> GerarResumoGeral(int usuarioId);
        Task<PlanoEstudo> ObterPlanoPorId(int planoId);
        Task<List<PlanoEstudo>> ListarPorUsuario(int usuarioId);
        Task Excluir (PlanoEstudo plano);
        Task<int> ContarPorUsuario(int usuarioId);
        Task<PlanoEstudo> ObterPlanoAtivo(int usuarioId);
    }
}