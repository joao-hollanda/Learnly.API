using Learnly.Domain.Entities;

namespace Learnly.Repository.Interfaces
{
    public interface IEventoEstudoRepositorio
    {
        Task<List<EventoEstudo>> ObterPorUsuario(int usuarioId);
        Task<EventoEstudo?> ObterPorId(int id);
        Task Adicionar(EventoEstudo evento);
        Task Remover(int id);
        Task AdicionarEmLote(List<EventoEstudo> eventos);
    }
}
