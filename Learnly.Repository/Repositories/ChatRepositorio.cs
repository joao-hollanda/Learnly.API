using Learnly.Domain.Entities.Social;
using Learnly.Domain.Enums;
using Learnly.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Repository.Repositories
{
    public class ChatRepositorio : BaseRepositorio, IChatRepositorio
    {
        public ChatRepositorio(LearnlyContexto contexto) : base(contexto)
        {
        }

        public async Task CriarConversa(Conversa conversa)
        {
            await _contexto.Conversas.AddAsync(conversa);
            await _contexto.SaveChangesAsync();
        }

        public async Task AtualizarConversa(Conversa conversa)
        {
            _contexto.Conversas.Update(conversa);
            await _contexto.SaveChangesAsync();
        }

        public async Task<Conversa?> ObterConversa(int conversaId)
        {
            return await _contexto.Conversas
                .Include(c => c.Participantes)
                .Include(c => c.Grupo)
                .FirstOrDefaultAsync(c => c.ConversaId == conversaId);
        }

        public async Task<Conversa?> ObterConversaDireta(int usuarioA, int usuarioB)
        {
            return await _contexto.Conversas
                .Include(c => c.Participantes)
                .Where(c => c.Tipo == ConversaTipo.Direta &&
                            c.Participantes.Any(p => p.UsuarioId == usuarioA) &&
                            c.Participantes.Any(p => p.UsuarioId == usuarioB))
                .FirstOrDefaultAsync();
        }

        public async Task<Conversa?> ObterConversaDoGrupo(int grupoId)
        {
            return await _contexto.Conversas
                .FirstOrDefaultAsync(c => c.Tipo == ConversaTipo.Grupo && c.GrupoId == grupoId);
        }

        public async Task<List<Conversa>> ListarConversasDiretas(int usuarioId)
        {
            return await _contexto.Conversas
                .Include(c => c.Participantes)
                    .ThenInclude(p => p.Usuario)
                .Where(c => c.Tipo == ConversaTipo.Direta &&
                            c.Participantes.Any(p => p.UsuarioId == usuarioId))
                .ToListAsync();
        }

        public async Task AdicionarMensagem(Mensagem mensagem)
        {
            await _contexto.Mensagens.AddAsync(mensagem);
            await _contexto.SaveChangesAsync();
        }

        public async Task<List<Mensagem>> ListarMensagens(int conversaId, int? antesDeId, int limite)
        {
            var query = _contexto.Mensagens
                .Include(m => m.Remetente)
                .Where(m => m.ConversaId == conversaId);

            if (antesDeId.HasValue)
                query = query.Where(m => m.MensagemId < antesDeId.Value);

            var mensagens = await query
                .OrderByDescending(m => m.MensagemId)
                .Take(limite)
                .ToListAsync();

            mensagens.Reverse();
            return mensagens;
        }

        public async Task<Mensagem?> ObterUltimaMensagem(int conversaId)
        {
            return await _contexto.Mensagens
                .Include(m => m.Remetente)
                .Where(m => m.ConversaId == conversaId)
                .OrderByDescending(m => m.MensagemId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> ContarNaoLidas(int conversaId, int usuarioId, DateTime? ultimaLeitura)
        {
            var query = _contexto.Mensagens
                .Where(m => m.ConversaId == conversaId && m.RemetenteId != usuarioId);

            if (ultimaLeitura.HasValue)
                query = query.Where(m => m.DataEnvio > ultimaLeitura.Value);

            return await query.CountAsync();
        }

        public async Task<ConversaParticipante?> ObterParticipante(int conversaId, int usuarioId)
        {
            return await _contexto.ConversaParticipantes
                .FirstOrDefaultAsync(p => p.ConversaId == conversaId && p.UsuarioId == usuarioId);
        }

        public async Task<List<ConversaParticipante>> ListarParticipantes(int conversaId)
        {
            return await _contexto.ConversaParticipantes
                .Include(p => p.Usuario)
                .Where(p => p.ConversaId == conversaId)
                .ToListAsync();
        }

        public async Task AtualizarParticipante(ConversaParticipante participante)
        {
            _contexto.ConversaParticipantes.Update(participante);
            await _contexto.SaveChangesAsync();
        }
    }
}
