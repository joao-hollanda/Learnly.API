using Learnly.Api.Models.Simulados.Request;
using Learnly.Api.Models.Simulados.Response;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities.Simulados;
using Learnly.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Learnly.API.Controllers;

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

        [HttpPost()]
        [Route("")]
        public async Task<IActionResult> CriarSimulado([FromBody] SimuladoRequest dto)
        {
            try
            {
                var usuarioId = GetUserId();
                if (usuarioId == null)
                {
                    return Forbid();
                }

                var simuladoDominio = new Simulado
                {
                    UsuarioId = (int)usuarioId,
                    Data = DateTime.UtcNow
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
                Console.WriteLine(ex);
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

                if (simulado.UsuarioId != GetUserId())
                    return Forbid("Este simulado pertence a outro usuário!");

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
                        AlternativaId = r.AlternativaId,
                        Explicacao = r.Explicacao
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
        [Route("Listar")]
        public async Task<IActionResult> Listar5()
        {
            try
            {
                var usuarioId = GetUserId();
                if (usuarioId != null)
                {
                    var simulados = await _simuladoAplicacao.Listar5((int)usuarioId);
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
                                AlternativaId = r.AlternativaId,
                                Explicacao = r.Explicacao
                            }).ToList()
                        };

                        simuladosListados.Add(dto);
                    }

                    return Ok(simuladosListados);

                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("Contar")]
        public async Task<IActionResult> ContarTotal()
        {
            try
            {
                var usuarioId = GetUserId();
                if (usuarioId != null)
                {
                    return Ok(await _simuladoAplicacao.Contar((int)usuarioId));
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}