using System.Text;
using Microsoft.EntityFrameworkCore;
using Learnly.Domain.Entities.Simulados;
using Learnly.Repository.Interfaces;
using Microsoft.VisualBasic;

namespace Learnly.Repository.Repositories
{
    public class SimuladoRepositorio : ISimuladoRepositorio
    {
        private readonly LearnlyContexto _context;

        public SimuladoRepositorio(LearnlyContexto context)
        {
            _context = context;
        }

        public async Task<int> GerarSimulado(Simulado simulado, List<SimuladoQuestao> questoes)
        {

            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.Simulados.Add(simulado);
            await _context.SaveChangesAsync();

            foreach (var q in questoes)
                q.SimuladoId = simulado.SimuladoId;

            _context.SimuladoQuestoes.AddRange(questoes);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return simulado.SimuladoId;
        }



        public async Task<List<Questao>> GerarQuestoesAsync(List<string> disciplinas, int totalQuestoes = 25)
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

                var questoes = await _context.Questoes
                                             .FromSqlRaw(
                                                 "SELECT * FROM Questoes WHERE Disciplina = {0} ORDER BY RANDOM() LIMIT {1}",
                                                 disciplina, limite)
                                             .Include(q => q.Alternativas)
                                             .ToListAsync();

                resultado.AddRange(questoes);
            }

            return resultado;
        }

        public async Task<Simulado> Obter(int simuladoId)
        {
            return await _context.Simulados
                .Include(s => s.Questoes)
                .ThenInclude(sq => sq.Questao)
                .ThenInclude(q => q.Alternativas)
                .Include(s => s.Respostas)
                .FirstOrDefaultAsync(s => s.SimuladoId == simuladoId);
        }

        public async Task<Questao> ObterQuestao(int questaoId)
        {
            return await _context.Questoes.FirstOrDefaultAsync(q => q.QuestaoId == questaoId);
        }
        public async Task<Alternativa> ObterAlternativa(int alternativaId)
        {
            return await _context.Alternativas.FirstOrDefaultAsync(a => a.AlternativaId == alternativaId);
        }

        public async Task ResponderSimulado(Simulado simulado)
        {
            _context.Simulados.Update(simulado);
            await _context.RespostasSimulado.AddRangeAsync(simulado.Respostas);
            await _context.SaveChangesAsync();
        }


        public async Task AtualizarSimuladoAsync(List<SimuladoQuestao> simuladoQuestoes)
        {
            _context.SimuladoQuestoes.AddRange(simuladoQuestoes);
            await _context.SaveChangesAsync();
        }
    }
}
