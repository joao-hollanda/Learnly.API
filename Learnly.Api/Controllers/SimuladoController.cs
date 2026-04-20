using Learnly.Api.Models.Simulados.Request;
using Learnly.Api.Models.Simulados.Response;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities.Simulados;
using Learnly.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SimuladoController : BaseController
    {
        private readonly ISimuladoAplicacao _simuladoAplicacao;

        public SimuladoController(ISimuladoAplicacao simuladoAplicacao)
        {
            _simuladoAplicacao = simuladoAplicacao;
        }

        [HttpPost]
        public async Task<IActionResult> CriarSimulado([FromBody] SimuladoRequest dto)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            var simuladoDominio = new Simulado
            {
                UsuarioId = (int)usuarioId,
                Data = DateTime.UtcNow
            };

            var simuladoId = await _simuladoAplicacao.GerarSimulado(simuladoDominio, dto.Disciplinas, dto.QuantidadeQuestoes);

            return Success(simuladoId);
        }

        [HttpPut("Responder/{simuladoId}")]
        public async Task<IActionResult> ResponderSimulado([FromRoute] int simuladoId, [FromBody] List<RespostaRequest> respostasSimulado)
        {
            var respostas = respostasSimulado.Select(r => new RespostaSimulado
            {
                SimuladoId = simuladoId,
                QuestaoId = r.QuestaoId,
                AlternativaId = r.AlternativaId
            }).ToList();

            var simuladoDominio = await _simuladoAplicacao.Obter(simuladoId);
            simuladoDominio.Respostas = respostas;

            var simulado = await _simuladoAplicacao.ResponderSimulado(simuladoDominio);

            return Success(new SimuladoCorrigido
            {
                Nota = simulado.NotaFinal,
                Desempenho = simulado.Desempenho
            });
        }

        [HttpGet("{simuladoId}")]
        public async Task<IActionResult> ObterSimulado([FromRoute] int simuladoId)
        {
            var simulado = await _simuladoAplicacao.Obter(simuladoId);

            if (simulado.UsuarioId != GetUserId())
                return Forbid("Este simulado pertence a outro usuário!");

            return Success(new SimuladoObter
            {
                SimuladoId = simulado.SimuladoId,
                NotaFinal = simulado.NotaFinal,
                Data = simulado.Data,

                Questoes = simulado.Questoes.Select(sq => new QuestaoSimuladoDto
                {
                    QuestaoId = sq.Questao.QuestaoId,
                    Titulo = sq.Questao.Titulo,
                    Disciplina = sq.Questao.Disciplina,
                    Arquivos = sq.Questao.Arquivos,
                    Contexto = sq.Questao.Contexto,
                    IntroducaoAlternativa = sq.Questao.IntroducaoAlternativa,
                    Alternativas = sq.Questao.Alternativas.Select(a => new AlternativaDto
                    {
                        AlternativaId = a.AlternativaId,
                        Letra = a.Letra,
                        Texto = a.Texto,
                        Correta = a.Correta,
                        Arquivo = a.Arquivo
                    }).ToList()
                }).ToList(),

                Respostas = simulado.Respostas.Select(r => new RespostaSimuladoDto
                {
                    QuestaoId = r.QuestaoId,
                    AlternativaId = r.AlternativaId,
                    Explicacao = r.Explicacao
                }).ToList()
            });
        }

        [HttpGet("Listar")]
        public async Task<IActionResult> Listar5()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            var simulados = await _simuladoAplicacao.Listar5((int)usuarioId);

            return Success(simulados.Select(simulado => new SimuladoObter
            {
                SimuladoId = simulado.SimuladoId,
                NotaFinal = simulado.NotaFinal,
                Data = simulado.Data,

                Questoes = simulado.Questoes.Select(sq => new QuestaoSimuladoDto
                {
                    QuestaoId = sq.Questao.QuestaoId,
                    Titulo = sq.Questao.Titulo,
                    Disciplina = sq.Questao.Disciplina,
                    Arquivos = sq.Questao.Arquivos,
                    Contexto = sq.Questao.Contexto,
                    IntroducaoAlternativa = sq.Questao.IntroducaoAlternativa,
                    Alternativas = sq.Questao.Alternativas.Select(a => new AlternativaDto
                    {
                        AlternativaId = a.AlternativaId,
                        Letra = a.Letra,
                        Texto = a.Texto,
                        Correta = a.Correta,
                        Arquivo = a.Arquivo
                    }).ToList()
                }).ToList(),

                Respostas = simulado.Respostas.Select(r => new RespostaSimuladoDto
                {
                    QuestaoId = r.QuestaoId,
                    AlternativaId = r.AlternativaId,
                    Explicacao = r.Explicacao
                }).ToList()
            }).ToList());
        }

        [HttpGet("Contar")]
        public async Task<IActionResult> ContarTotal()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            return Success(await _simuladoAplicacao.Contar((int)usuarioId));
        }
    }
}
