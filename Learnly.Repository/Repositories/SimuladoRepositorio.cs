using Microsoft.EntityFrameworkCore;
using Learnly.Domain.Entities.Simulados;
using Learnly.Repository.Interfaces;

namespace Learnly.Repository.Repositories
{
    public class SimuladoRepositorio : BaseRepositorio, ISimuladoRepositorio
    {
        public SimuladoRepositorio(LearnlyContexto contexto) : base(contexto)
        {
        }

        public async Task<int> GerarSimulado(Simulado simulado, List<SimuladoQuestao> questoes)
        {
            var strategy = _contexto.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _contexto.Database.BeginTransactionAsync();

                try
                {
                    _contexto.Simulados.Add(simulado);
                    await _contexto.SaveChangesAsync();

                    foreach (var q in questoes)
                        q.SimuladoId = simulado.SimuladoId;

                    _contexto.SimuladoQuestoes.AddRange(questoes);
                    await _contexto.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return simulado.SimuladoId;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        public async Task<List<Questao>> GerarQuestoesAsync(List<string> disciplinas, int totalQuestoes)
        {
            if (disciplinas == null || disciplinas.Count == 0)
                throw new ArgumentException("A lista de disciplinas não pode estar vazia.");

            var resultado = new List<Questao>();

            int n = disciplinas.Count;
            int baseQtd = totalQuestoes / n;
            int restante = totalQuestoes % n;

            for (int i = 0; i < n; i++)
            {
                var disciplina = disciplinas[i];
                int limite = baseQtd + (restante > 0 ? 1 : 0);
                if (restante > 0) restante--;

                var questoes = await _contexto.Questoes
                    .Where(q => q.Disciplina == disciplina)
                    .OrderBy(q => EF.Functions.Random())
                    .Take(limite)
                    .Include(q => q.Alternativas)
                    .ToListAsync();

                resultado.AddRange(questoes);
            }

            return resultado;
        }

        public async Task<Simulado> Obter(int simuladoId)
        {
            return await _contexto.Simulados
                .Include(s => s.Questoes)
                    .ThenInclude(sq => sq.Questao)
                        .ThenInclude(q => q.Alternativas)
                .Include(s => s.Respostas)
                .FirstOrDefaultAsync(s => s.SimuladoId == simuladoId);
        }

        public async Task<List<Simulado>> Listar(int usuarioId, int quantidade)
        {
            return await _contexto.Simulados
                .Include(s => s.Questoes)
                    .ThenInclude(sq => sq.Questao)
                        .ThenInclude(q => q.Alternativas)
                .Include(s => s.Respostas)
                .Where(s => s.UsuarioId == usuarioId)
                .OrderByDescending(s => s.Data)
                .Take(quantidade)
                .ToListAsync();
        }

        public async Task<List<(int SimuladoId, decimal NotaFinal, DateTime Data, int QuantidadeQuestoes)>> ListarResumo(int usuarioId, int quantidade)
        {
            var dados = await _contexto.Simulados
                .AsNoTracking()
                .Where(s => s.UsuarioId == usuarioId)
                .OrderByDescending(s => s.Data)
                .Take(quantidade)
                .Select(s => new
                {
                    s.SimuladoId,
                    s.NotaFinal,
                    s.Data,
                    Quantidade = s.Questoes.Count
                })
                .ToListAsync();

            return dados
                .Select(d => (d.SimuladoId, d.NotaFinal, d.Data, d.Quantidade))
                .ToList();
        }

        public async Task<Questao> ObterQuestao(int questaoId)
        {
            return await _contexto.Questoes
                .Include(q => q.Alternativas)
                .FirstOrDefaultAsync(q => q.QuestaoId == questaoId);
        }

        public async Task<Alternativa> ObterAlternativa(int alternativaId)
        {
            return await _contexto.Alternativas
                .FirstOrDefaultAsync(a => a.AlternativaId == alternativaId);
        }

        public async Task ResponderSimulado(Simulado simulado)
        {
            _contexto.Simulados.Update(simulado);
            await _contexto.RespostasSimulado.AddRangeAsync(simulado.Respostas);
            await _contexto.SaveChangesAsync();
        }

        public async Task AtualizarSimuladoAsync(List<SimuladoQuestao> simuladoQuestoes)
        {
            _contexto.SimuladoQuestoes.AddRange(simuladoQuestoes);
            await _contexto.SaveChangesAsync();
        }

        public async Task<int> ContarTotal(int usuarioId)
        {
            return await _contexto.Simulados
                .Where(s => s.UsuarioId == usuarioId)
                .CountAsync();
        }

        public async Task<List<RespostaSimulado>> ListarRespostasComQuestao(int usuarioId)
        {
            return await _contexto.RespostasSimulado
                .AsNoTracking()
                .Include(r => r.Questao)
                .Include(r => r.Alternativa)
                .Where(r => r.Simulado.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<List<Simulado>> ListarNotas(int usuarioId)
        {
            return await _contexto.Simulados
                .AsNoTracking()
                .Where(s => s.UsuarioId == usuarioId && s.Respostas.Any())
                .OrderBy(s => s.Data)
                .ToListAsync();
        }
    }
}
