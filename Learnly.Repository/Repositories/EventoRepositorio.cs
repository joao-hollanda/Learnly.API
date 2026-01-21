using Learnly.Domain.Entities;
using Learnly.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infra.Data.Repositories
{
    public class EventoEstudoRepositorio : IEventoEstudoRepositorio
    {
        private readonly LearnlyContexto _context;

        public EventoEstudoRepositorio(LearnlyContexto context)
        {
            _context = context;
        }

        public async Task<List<EventoEstudo>> ObterPorUsuario(int usuarioId)
        {
            return await _context.EventosEstudo
                .Where(e => e.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<EventoEstudo?> ObterPorId(int id)
        {
            return await _context.EventosEstudo.FindAsync(id);
        }

        public async Task Adicionar(EventoEstudo evento)
        {
            _context.EventosEstudo.Add(evento);
            await _context.SaveChangesAsync();
        }

        public async Task Remover(int id)
        {
            var eventos = await _context.EventosEstudo.Where(e => e.UsuarioId == id).ToListAsync();

            _context.EventosEstudo.RemoveRange(eventos);
            await _context.SaveChangesAsync();
        }

        public async Task AdicionarEmLote(List<EventoEstudo> eventos)
        {
            _context.EventosEstudo.AddRange(eventos);
            await _context.SaveChangesAsync();
        }

    }
}
