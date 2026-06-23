using Microsoft.EntityFrameworkCore;
using Learnly.Domain.Entities.Redacoes;
using Learnly.Repository.Interfaces;

namespace Learnly.Repository.Repositories
{
    public class RedacaoRepositorio : BaseRepositorio, IRedacaoRepositorio
    {
        public RedacaoRepositorio(LearnlyContexto contexto) : base(contexto)
        {
        }

        public async Task<int> Criar(Redacao redacao)
        {
            _contexto.Redacoes.Add(redacao);
            await _contexto.SaveChangesAsync();
            return redacao.RedacaoId;
        }

        public async Task<List<Redacao>> Listar(int usuarioId)
        {
            return await _contexto.Redacoes
                .AsNoTracking()
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<Redacao?> Obter(int redacaoId)
        {
            return await _contexto.Redacoes
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RedacaoId == redacaoId);
        }
    }
}
