using Learnly.Api.Models.Simulados.Request;
using Learnly.Api.Models.Simulados.Response;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities.Simulados;
using Learnly.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimuladoController : ControllerBase
    {
        private readonly ISimuladoAplicacao _simuladoAplicacao;

        public SimuladoController(ISimuladoAplicacao simuladoAplicacao)
        {
            _simuladoAplicacao = simuladoAplicacao;
        }

        [HttpPost()]
        [Route("{usuarioId}")]
        public async Task<IActionResult> CriarSimulado([FromRoute] int usuarioId, [FromBody] SimuladoRequest dto)
        {
            try
            {
                var simuladoDominio = new Simulado
                {
                    UsuarioId = usuarioId,
                    Data = DateTime.Now
                };

                var simuladoId = await _simuladoAplicacao.GerarSimulado(simuladoDominio, dto.Disciplinas, dto.QuantidadeQuestoes);

                return Ok(simuladoId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut()]
        [Route("Responder/{simuladoId}")]

        public async Task<IActionResult> ResponderSimulado([FromRoute] int simuladoId, [FromBody] List<RespostaRequest> respostasSimulado)
        {
            try
            {
                var respostas = new List<RespostaSimulado>();

                foreach (var resposta in respostasSimulado)
                {
                    respostas.Add(
                        new RespostaSimulado
                        {
                            SimuladoId = simuladoId,
                            QuestaoId = resposta.QuestaoId,
                            AlternativaId = resposta.AlternativaId
                        }
                    );
                }

                var simuladoDominio = await _simuladoAplicacao.Obter(simuladoId);

                simuladoDominio.Respostas = respostas;

                var simulado = await _simuladoAplicacao.ResponderSimulado(simuladoDominio);

                var simuladoResposta = new SimuladoCorrigido
                {
                    Nota = simulado.NotaFinal,
                    Desempenho = simulado.Desempenho
                };

                return Ok(simuladoResposta);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("{simuladoId}")]
        public async Task<IActionResult> ObterSimulado([FromRoute] int simuladoId)
        {
            try
            {
                var simulado = await _simuladoAplicacao.Obter(simuladoId);

                if (simulado == null)
                    return NotFound("Simulado não encontrado");

                var dto = new SimuladoObter
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
                        AlternativaId = r.AlternativaId
                    }).ToList()
                };

                return Ok(dto);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("Listar/{usuarioId}")]
        public async Task<IActionResult> Listar5([FromRoute] int usuarioId)
        {
            try
            {
                var simulados = await _simuladoAplicacao.Listar5(usuarioId);
                var simuladosListados = new List<SimuladoObter>();

                foreach (Simulado simulado in simulados)
                {
                    var dto = new SimuladoObter
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
                            AlternativaId = r.AlternativaId
                        }).ToList()
                    };

                    simuladosListados.Add(dto);
                }

                return Ok(simuladosListados);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}