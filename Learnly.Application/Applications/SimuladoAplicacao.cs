using FluentValidation;
using Learnly.Application.DTOs;
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
        readonly IExplicacaoQuestaoRepositorio _explicacaoQuestaoRepositorio;
        readonly IUsuarioRepositorio _usuarioRepositorio;
        readonly IIAService _iaService;
        readonly IValidator<Simulado> _validator;

        public SimuladoAplicacao(
            ISimuladoRepositorio simuladoRepositorio,
            IExplicacaoQuestaoRepositorio explicacaoQuestaoRepositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IIAService iaService,
            IValidator<Simulado> validator)
        {
            _simuladoRepositorio = simuladoRepositorio;
            _explicacaoQuestaoRepositorio = explicacaoQuestaoRepositorio;
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

            // Gera/persiste as explicações antes de anexar as respostas ao simulado rastreado:
            // o SaveChanges do cache compartilha o mesmo DbContext e, com as respostas já anexadas,
            // dispararia um insert prematuro delas, duplicando-as no save final.
            var explicacoes = await ObterOuGerarExplicacoes(questoesErradas);
            var mapaExplicacoes = explicacoes.ToDictionary(e => e.QuestaoId, e => e.Explicacao);

            foreach (var resposta in respostas)
            {
                if (mapaExplicacoes.TryGetValue(resposta.QuestaoId, out var explicacao))
                    resposta.Explicacao = explicacao;
            }

            simuladoBanco.Respostas = respostas;
            simuladoBanco.Desempenho = desempenho;
            simuladoBanco.Desempenho.Feedback = await _iaService.GerarFeedbackAsync(simuladoBanco);

            try
            {
                simuladoBanco.MateriaisRecomendados = await _iaService.GerarMateriaisAsync(simuladoBanco);
            }
            catch
            {
                simuladoBanco.MateriaisRecomendados = new List<MaterialRecomendado>();
            }

            simuladoBanco.NotaFinal = desempenho.QuantidadeDeQuestoes > 0
                ? Math.Round((decimal)desempenho.QuantidadeDeAcertos / desempenho.QuantidadeDeQuestoes * 10, 2)
                : 0;

            await _simuladoRepositorio.ResponderSimulado(simuladoBanco);

            return simuladoBanco;
        }

        public async Task<List<ExplicacaoQuestao>> ObterOuGerarExplicacoes(List<SimuladoQuestao> questoesErradas)
        {
            if (questoesErradas == null || !questoesErradas.Any())
                return new List<ExplicacaoQuestao>();

            var ids = questoesErradas.Select(q => q.QuestaoId).ToList();

            var existentes = await _explicacaoQuestaoRepositorio.ObterPorQuestoes(ids);
            var mapa = existentes.ToDictionary(e => e.QuestaoId);

            var faltantes = questoesErradas
                .Where(q => !mapa.ContainsKey(q.QuestaoId))
                .ToList();

            if (faltantes.Any())
            {
                var novas = await _iaService.GerarExplicacoesAsync(faltantes);
                await _explicacaoQuestaoRepositorio.Salvar(novas);

                foreach (var nova in novas)
                    mapa[nova.QuestaoId] = nova;
            }

            return mapa.Values.ToList();
        }

        public async Task<Simulado> Obter(int simuladoId, int usuarioId)
        {
            var simulado = await _simuladoRepositorio.Obter(simuladoId)
                ?? throw new SimuladoNaoEncontradoException(simuladoId);

            if (simulado.UsuarioId != usuarioId)
                throw new SimuladoNaoAutorizadoException();

            return simulado;
        }

        public async Task<List<Simulado>> Listar(int usuarioId, int quantidade = 5)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException();

            return await _simuladoRepositorio.Listar(usuarioId, quantidade);
        }

        public async Task<List<SimuladoResumoDto>> ListarResumo(int usuarioId, int quantidade = 9)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException();

            var resumos = await _simuladoRepositorio.ListarResumo(usuarioId, quantidade);

            return resumos.Select(r => new SimuladoResumoDto
            {
                SimuladoId = r.SimuladoId,
                NotaFinal = r.NotaFinal,
                Data = r.Data,
                QuantidadeQuestoes = r.QuantidadeQuestoes
            }).ToList();
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