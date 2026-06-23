using Microsoft.EntityFrameworkCore;
using Learnly.Domain.Entities.Simulados;
using Learnly.Repository.Interfaces;

namespace Learnly.Repository.Repositories
{
    public class ExplicacaoQuestaoRepositorio : BaseRepositorio, IExplicacaoQuestaoRepositorio
    {
        public ExplicacaoQuestaoRepositorio(LearnlyContexto contexto) : base(contexto)
        {
        }

        public async Task<List<ExplicacaoQuestao>> ObterPorQuestoes(IEnumerable<int> questaoIds)
        {
            var ids = questaoIds.ToList();

            return await _contexto.ExplicacoesQuestao
                .AsNoTracking()
                .Where(e => ids.Contains(e.QuestaoId))
                .ToListAsync();
        }

        public async Task Salvar(IEnumerable<ExplicacaoQuestao> explicacoes)
        {
            var novas = explicacoes.ToList();
            if (!novas.Any()) return;

            await _contexto.ExplicacoesQuestao.AddRangeAsync(novas);
            await _contexto.SaveChangesAsync();
        }
    }
}
