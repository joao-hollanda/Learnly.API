using Learnly.Domain.Entities.Social;
using Learnly.Domain.Enums;
using Learnly.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Repository.Repositories
{
    public class AmizadeRepositorio : BaseRepositorio, IAmizadeRepositorio
    {
        public AmizadeRepositorio(LearnlyContexto contexto) : base(contexto)
        {
        }

        public async Task Criar(Amizade amizade)
        {
            await _contexto.Amizades.AddAsync(amizade);
            await _contexto.SaveChangesAsync();
        }

        public async Task Atualizar(Amizade amizade)
        {
            _contexto.Amizades.Update(amizade);
            await _contexto.SaveChangesAsync();
        }

        public async Task Remover(Amizade amizade)
        {
            _contexto.Amizades.Remove(amizade);
            await _contexto.SaveChangesAsync();
        }

        public async Task<Amizade?> Obter(int amizadeId)
        {
            return await _contexto.Amizades
                .Include(a => a.Solicitante)
                .Include(a => a.Destinatario)
                .FirstOrDefaultAsync(a => a.AmizadeId == amizadeId);
        }

        public async Task<Amizade?> ObterEntre(int usuarioA, int usuarioB)
        {
            return await _contexto.Amizades
                .FirstOrDefaultAsync(a =>
                    (a.SolicitanteId == usuarioA && a.DestinatarioId == usuarioB) ||
                    (a.SolicitanteId == usuarioB && a.DestinatarioId == usuarioA));
        }

        public async Task<List<Amizade>> ListarAceitas(int usuarioId)
        {
            return await _contexto.Amizades
                .Include(a => a.Solicitante)
                .Include(a => a.Destinatario)
                .Where(a => a.Status == AmizadeStatus.Aceita &&
                            (a.SolicitanteId == usuarioId || a.DestinatarioId == usuarioId))
                .ToListAsync();
        }

        public async Task<List<Amizade>> ListarPendentesRecebidas(int usuarioId)
        {
            return await _contexto.Amizades
                .Include(a => a.Solicitante)
                .Where(a => a.Status == AmizadeStatus.Pendente && a.DestinatarioId == usuarioId)
                .OrderByDescending(a => a.DataSolicitacao)
                .ToListAsync();
        }

        public async Task<List<Amizade>> ListarPendentesEnviadas(int usuarioId)
        {
            return await _contexto.Amizades
                .Include(a => a.Destinatario)
                .Where(a => a.Status == AmizadeStatus.Pendente && a.SolicitanteId == usuarioId)
                .OrderByDescending(a => a.DataSolicitacao)
                .ToListAsync();
        }
    }
}
