using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Simulados;
using Learnly.Domain.Exceptions.Simulados;
using Learnly.Domain.Exceptions.Usuarios;
using Learnly.Repository.Interfaces;
using Learnly.Services.Interfaces;

namespace Learnly.Application.Applications
{
    public class SimuladoAplicacao : ISimuladoAplicacao
    {
        readonly ISimuladoRepositorio _simuladoRepositorio;
        readonly IUsuarioRepositorio _usuarioRepositorio;
        readonly IIAService _iaService;

        public SimuladoAplicacao(ISimuladoRepositorio simuladoRepositorio, IUsuarioRepositorio usuarioRepositorio, IIAService iaService)
        {
            _simuladoRepositorio = simuladoRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _iaService = iaService;
        }

        public async Task<int> GerarSimulado(Simulado simulado, List<string> disciplinas, int totalQuestoes = 25)
        {
            if (simulado == null)
                throw new ArgumentException("Simulado não pode ser nulo.");

            var usuario = await _usuarioRepositorio.Obter(simulado.UsuarioId, true);
            if (usuario == null)
                throw new UsuarioNaoEncontradoException(simulado.UsuarioId);

            var questoes = await _simuladoRepositorio.GerarQuestoesAsync(disciplinas, totalQuestoes);

            var simuladoQuestoes = questoes.Select(q => new SimuladoQuestao
            {
                QuestaoId = q.QuestaoId
            }).ToList();

            var simuladoId = await _simuladoRepositorio.GerarSimulado(simulado, simuladoQuestoes);

            return simuladoId;
        }

        public async Task<Simulado> ResponderSimulado(Simulado simulado)
        {
            if (simulado == null)
                throw new ArgumentException("Simulado não encontrado!");

            var simuladoBanco = await _simuladoRepositorio.Obter(simulado.SimuladoId);

            if (simuladoBanco == null)
                throw new SimuladoNaoEncontradoException(simulado.SimuladoId);

            if (simulado.Respostas == null || !simulado.Respostas.Any())
                throw new RespostasNaoInformadasException();

            var desempenho = new DesempenhoSimulado();

            foreach (var resposta in simulado.Respostas)
            {
                resposta.Questao = await _simuladoRepositorio.ObterQuestao(resposta.QuestaoId);
                resposta.Alternativa = await _simuladoRepositorio.ObterAlternativa(resposta.AlternativaId);

                if (resposta.Questao == null)
                    throw new QuestaoNaoEncontradaException(resposta.QuestaoId);

                if (resposta.Alternativa == null)
                    throw new AlternativaNaoEncontradaException(resposta.AlternativaId);

                if (resposta.Questao.AlternativaCorreta == resposta.Alternativa.Letra)
                    resposta.Alternativa.Correta = true;
            }

            desempenho.QuantidadeDeQuestoes = simuladoBanco.Questoes.Count;
            desempenho.QuantidadeDeAcertos = simulado.Respostas.Count(r => r.Alternativa.Correta);

            simuladoBanco.Respostas = simulado.Respostas;
            simuladoBanco.Desempenho = desempenho;

            simuladoBanco.Desempenho.Feedback =
                await _iaService.GerarFeedbackAsync(simuladoBanco);

            var questoesErradas = simuladoBanco.Questoes
                .Where(q =>
                {
                    if (q.Questao == null) return false;

                    if (q.Questao.Arquivos != null) return false;

                    var resposta = simuladoBanco.Respostas.FirstOrDefault(r => r.QuestaoId == q.QuestaoId);
                    if (resposta == null) return false;

                    var alternativaCorreta = q.Questao.Alternativas
                        .FirstOrDefault(a => a.Letra == q.Questao.AlternativaCorreta);
                    if (alternativaCorreta == null) return false;

                    return resposta.AlternativaId != alternativaCorreta.AlternativaId;
                })
                .ToList();

            var respostas = simuladoBanco.Respostas
                            .ToDictionary(r => r.QuestaoId);

            var respostasComExplicacao = await _iaService.GerarExplicacoes(questoesErradas, respostas);

            foreach (var resposta in respostasComExplicacao)
            {
                var respostaSimulado = simulado.Respostas.FirstOrDefault(r => r.QuestaoId == resposta.QuestaoId);
                if (respostaSimulado != null)
                    respostaSimulado.Explicacao = resposta.Explicacao;
            }

            simuladoBanco.NotaFinal =
                desempenho.QuantidadeDeAcertos /
                desempenho.QuantidadeDeQuestoes * 10;

            await _simuladoRepositorio.ResponderSimulado(simuladoBanco);

            return simuladoBanco;
        }
        public async Task<Simulado> Obter(int id)
        {
            var simuladoDominio = await _simuladoRepositorio.Obter(id);

            if (simuladoDominio == null)
                throw new SimuladoNaoEncontradoException(id);

            return simuladoDominio;
        }
        public async Task<List<Simulado>> Listar5(int id)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(id, true);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException();

            return await _simuladoRepositorio.Listar5(id);
        }

        public async Task<int> Contar(int usuarioId)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new Exception("Usuário não encontrado!");

            return await _simuladoRepositorio.ContarTotal(usuarioId);
        }
    }
}