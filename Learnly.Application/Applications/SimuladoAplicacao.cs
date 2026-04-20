using FluentValidation;
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
        readonly IValidator<Simulado> _validator;

        public SimuladoAplicacao(ISimuladoRepositorio simuladoRepositorio, IUsuarioRepositorio usuarioRepositorio, IIAService iaService, IValidator<Simulado> validator)
        {
            _simuladoRepositorio = simuladoRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _iaService = iaService;
            _validator = validator;
        }

        public async Task<int> GerarSimulado(Simulado simulado, List<string> disciplinas, int totalQuestoes = 25)
        {
            if (simulado == null)
                throw new ArgumentException("Simulado não pode ser nulo.");

            await _validator.ValidateAndThrowAsync(simulado);

            var usuario = await _usuarioRepositorio.Obter(simulado.UsuarioId, true);
            if (usuario == null)
                throw new UsuarioNaoEncontradoException(simulado.UsuarioId);

            var questoes = await _simuladoRepositorio.GerarQuestoesAsync(disciplinas, totalQuestoes);

            var simuladoQuestoes = questoes.Select(q => new SimuladoQuestao
            {
                QuestaoId = q.QuestaoId
            }).ToList();

            return await _simuladoRepositorio.GerarSimulado(simulado, simuladoQuestoes);
        }

        public async Task<Simulado> ResponderSimulado(int simuladoId, List<RespostaSimulado> respostas, int usuarioId)
        {
            if (respostas == null || !respostas.Any())
                throw new RespostasNaoInformadasException();

            var simuladoBanco = await _simuladoRepositorio.Obter(simuladoId)
                ?? throw new SimuladoNaoEncontradoException(simuladoId);

            if (simuladoBanco.UsuarioId != usuarioId)
                throw new SimuladoNaoAutorizadoException();

            var desempenho = new DesempenhoSimulado();

            foreach (var resposta in respostas)
            {
                resposta.SimuladoId = simuladoId;
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
            desempenho.QuantidadeDeAcertos = respostas.Count(r => r.Alternativa.Correta);

            simuladoBanco.Respostas = respostas;
            simuladoBanco.Desempenho = desempenho;
            simuladoBanco.Desempenho.Feedback = await _iaService.GerarFeedbackAsync(simuladoBanco);

            var questoesErradas = simuladoBanco.Questoes
                .Where(q =>
                {
                    if (q.Questao == null || q.Questao.Arquivos != null) return false;
                    var resposta = respostas.FirstOrDefault(r => r.QuestaoId == q.QuestaoId);
                    if (resposta == null) return false;
                    var alternativaCorreta = q.Questao.Alternativas.FirstOrDefault(a => a.Letra == q.Questao.AlternativaCorreta);
                    if (alternativaCorreta == null) return false;
                    return resposta.AlternativaId != alternativaCorreta.AlternativaId;
                })
                .ToList();

            var respostasDict = respostas.ToDictionary(r => r.QuestaoId);
            var respostasComExplicacao = await _iaService.GerarExplicacoes(questoesErradas, respostasDict);

            foreach (var explicacao in respostasComExplicacao)
            {
                var respostaSimulado = respostas.FirstOrDefault(r => r.QuestaoId == explicacao.QuestaoId);
                if (respostaSimulado != null)
                    respostaSimulado.Explicacao = explicacao.Explicacao;
            }

            simuladoBanco.NotaFinal = desempenho.QuantidadeDeQuestoes > 0
                ? Math.Round((decimal)desempenho.QuantidadeDeAcertos / desempenho.QuantidadeDeQuestoes * 10, 2)
                : 0;

            await _simuladoRepositorio.ResponderSimulado(simuladoBanco);

            return simuladoBanco;
        }

        public async Task<Simulado> Obter(int simuladoId, int usuarioId)
        {
            var simulado = await _simuladoRepositorio.Obter(simuladoId)
                ?? throw new SimuladoNaoEncontradoException(simuladoId);

            if (simulado.UsuarioId != usuarioId)
                throw new SimuladoNaoAutorizadoException();

            return simulado;
        }

        public async Task<List<Simulado>> Listar5(int usuarioId)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException();

            return await _simuladoRepositorio.Listar5(usuarioId);
        }

        public async Task<int> Contar(int usuarioId)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException(usuarioId);

            return await _simuladoRepositorio.ContarTotal(usuarioId);
        }
    }
}
