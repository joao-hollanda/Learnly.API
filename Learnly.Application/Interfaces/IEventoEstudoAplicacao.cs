using Learnly.Application.DTOs;
using Learnly.Domain.Entities;

namespace Learnly.Application.Interfaces
{
    public interface IEventoEstudoAplicacao
    {
        Task<List<EventoEstudo>> Listar(int usuarioId);
        Task Criar(string titulo, DateTime inicio, DateTime fim, int usuarioId);
        Task Remover(int eventoId);
        Task CriarEmLote(int usuarioId, List<CriarEventoEstudoDto> eventosDto);
    }
}
