using Learnly.Application.DTOs;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;

namespace Learnly.Application.Interfaces
{
    public interface IPlanoAplicacao
    {
        Task<PlanoEstudo> Criar(CriarPlanoIADTO dto);
        Task<PlanoEstudo> Obter(int planoId);
        Task<List<PlanoEstudo>> Listar5(int usuarioId);
        Task Atualizar(PlanoEstudo planoEstudo);
        Task AtivarPlano(int planoId, int usuarioId);
        Task AdicionarMateria(int planoId, int materiaId, int horasTotais);
        Task LancarHoras(int planoMateriaId, int horas);
        Task<ResumoGeralDto> GerarResumo(int usuarioId);
        Task DesativarPlano(int planoId);
        Task<ComparacaoHorasDto> CompararHorasHojeOntem(int usuarioId);
        Task Excluir(int planoId);
        Task<PlanoEstudo> ObterPlanoAtivo(int usuarioId);
    }
}
