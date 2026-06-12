using Learnly.Domain.Entities;
using Learnly.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Repository.Repositories
{
    public class EventoEstudoRepositorio : BaseRepositorio, IEventoEstudoRepositorio
    {
        public EventoEstudoRepositorio(LearnlyContexto contexto) : base(contexto)
        {
        }

        public async Task<List<EventoEstudo>> ObterPorUsuario(int usuarioId)
        {
            return await _contexto.EventosEstudo
                .Where(e => e.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<EventoEstudo?> ObterPorId(int id)
        {
            return await _contexto.EventosEstudo.FindAsync(id);
        }

        public async Task Adicionar(EventoEstudo evento)
        {
            _contexto.EventosEstudo.Add(evento);
            await _contexto.SaveChangesAsync();
        }

        public async Task Remover(int id)
        {
            var eventos = await _contexto.EventosEstudo.Where(e => e.UsuarioId == id).ToListAsync();

            _contexto.EventosEstudo.RemoveRange(eventos);
            await _contexto.SaveChangesAsync();
        }

        public async Task AdicionarEmLote(List<EventoEstudo> eventos)
        {
            _contexto.EventosEstudo.AddRange(eventos);
            await _contexto.SaveChangesAsync();
        }

    }
}
