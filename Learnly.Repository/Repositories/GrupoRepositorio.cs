using Learnly.Domain.Entities.Social;
using Learnly.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Repository.Repositories
{
    public class GrupoRepositorio : BaseRepositorio, IGrupoRepositorio
    {
        public GrupoRepositorio(LearnlyContexto contexto) : base(contexto)
        {
        }

        public async Task Criar(Grupo grupo)
        {
            await _contexto.Grupos.AddAsync(grupo);
            await _contexto.SaveChangesAsync();
        }

        public async Task<Grupo?> Obter(int grupoId)
        {
            return await _contexto.Grupos
                .Include(g => g.Membros)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(g => g.GrupoId == grupoId);
        }

        public async Task<Grupo?> ObterPorChave(string chave)
        {
            return await _contexto.Grupos
                .Include(g => g.Membros)
                .FirstOrDefaultAsync(g => g.Chave == chave);
        }

        public async Task<bool> ChaveExiste(string chave)
        {
            return await _contexto.Grupos.AnyAsync(g => g.Chave == chave);
        }

        public async Task<List<Grupo>> ListarPorUsuario(int usuarioId)
        {
            return await _contexto.Grupos
                .Include(g => g.Membros)
                .Where(g => g.Membros.Any(m => m.UsuarioId == usuarioId))
                .OrderByDescending(g => g.GrupoId)
                .ToListAsync();
        }

        public async Task<GrupoMembro?> ObterMembro(int grupoId, int usuarioId)
        {
            return await _contexto.GrupoMembros
                .FirstOrDefaultAsync(m => m.GrupoId == grupoId && m.UsuarioId == usuarioId);
        }

        public async Task AdicionarMembro(GrupoMembro membro)
        {
            await _contexto.GrupoMembros.AddAsync(membro);
            await _contexto.SaveChangesAsync();
        }

        public async Task RemoverMembro(GrupoMembro membro)
        {
            _contexto.GrupoMembros.Remove(membro);
            await _contexto.SaveChangesAsync();
        }

        public async Task AtualizarMembro(GrupoMembro membro)
        {
            _contexto.GrupoMembros.Update(membro);
            await _contexto.SaveChangesAsync();
        }

        public async Task<bool> EhMembro(int grupoId, int usuarioId)
        {
            return await _contexto.GrupoMembros
                .AnyAsync(m => m.GrupoId == grupoId && m.UsuarioId == usuarioId);
        }
    }
}
