using Learnly.Domain.Entities.Simulados;

namespace Learnly.Repository.Interfaces
{
    public interface IExplicacaoQuestaoRepositorio
    {
        Task<List<ExplicacaoQuestao>> ObterPorQuestoes(IEnumerable<int> questaoIds);
        Task Salvar(IEnumerable<ExplicacaoQuestao> explicacoes);
    }
}
