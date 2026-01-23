using Learnly.Domain.Entities;

namespace Learnly.Application.Interfaces
{
    public interface IPlanoAplicacao
    {
        Task Criar(PlanoEstudo plano);
        Task<PlanoEstudo> Obter(int planoId);
        Task<List<PlanoEstudo>> Listar5(int usuarioId);
        Task Atualizar(PlanoEstudo planoEstudo);
        Task AtivarPlano(int planoId, int usuarioId);
        Task AdicionarMateria(int planoId, int materiaId, int horasTotais);
        Task LancarHoras(int planoMateriaId, int horas);
        Task<ResumoGeralDto> GerarResumo(int usuarioId);
        Task DesativarPlano(PlanoEstudo plano);
        Task<ComparacaoHorasDto> CompararHorasHojeOntem(int usuarioId);
        Task Excluir (PlanoEstudo plano);
        Task<PlanoEstudo> ObterPlanoAtivo (int usuarioId);
    }
}
