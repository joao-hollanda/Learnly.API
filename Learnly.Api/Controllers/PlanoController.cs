using Learnly.Api.Models.Planos.Request;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Learnly.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlanoController : BaseController
    {
        private readonly IPlanoAplicacao _planoAplicacao;
        private readonly IIAService _iaService;

        public PlanoController(IPlanoAplicacao planoAplicacao, IIAService iaService)
        {
            _planoAplicacao = planoAplicacao;
            _iaService = iaService;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarPlanoDTO dto)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            if (dto.PlanoIa)
            {
                var dtoPlano = new CriarPlanoIADTO
                {
                    Titulo = dto.Titulo,
                    Objetivo = dto.Objetivo,
                    DataInicio = dto.DataInicio,
                    DataFim = dto.DataFim,
                    HorasPorSemana = dto.HorasPorSemana,
                    UsuarioId = (int)usuarioId
                };

                PlanoEstudo planoGerado = await _iaService.GerarPlanoIA(dtoPlano);
                planoGerado.Titulo = dto.Titulo;
                planoGerado.Objetivo = dto.Objetivo;
                planoGerado.DataInicio = dto.DataInicio;
                planoGerado.DataFim = dto.DataFim;
                planoGerado.HorasPorSemana = dto.HorasPorSemana;
                planoGerado.UsuarioId = usuarioId.Value;

                await _planoAplicacao.Criar(planoGerado);
                return Ok(planoGerado);
            }

            var plano = new PlanoEstudo
            {
                Titulo = dto.Titulo,
                Objetivo = dto.Objetivo,
                UsuarioId = usuarioId.Value,
                DataInicio = DateTime.SpecifyKind(dto.DataInicio, DateTimeKind.Utc),
                DataFim = DateTime.SpecifyKind(dto.DataFim, DateTimeKind.Utc),
                HorasPorSemana = 0,
            };

            await _planoAplicacao.Criar(plano);
            return Ok(plano);
        }


        [HttpGet("{planoId}")]
        public async Task<IActionResult> Obter(int planoId)
        {
            var plano = await _planoAplicacao.Obter(planoId);
            if (plano.UsuarioId != GetUserId())
                return Forbid();

            return Ok(plano);
        }

        [HttpGet()]
        public async Task<IActionResult> Listar5()
        {
            var usuarioId = GetUserId();
            if (usuarioId != null)
            {
                var planos = await _planoAplicacao.Listar5((int)usuarioId);
                return Ok(planos);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpGet("gerar-resumo")]
        public async Task<ActionResult<ResumoGeralDto>> ObterResumoGeral()
        {
            var usuarioId = GetUserId();
            if (usuarioId != null)
            {
                return Ok(await _planoAplicacao.GerarResumo((int)usuarioId));
            }
            else
            {
                return Forbid();
            }
        }


        [HttpPut]
        public async Task<IActionResult> Atualizar([FromBody] PlanoEstudo plano)
        {
            await _planoAplicacao.Atualizar(plano);
            return NoContent();
        }

        [HttpPut("{planoId}/ativar")]
        public async Task<IActionResult> AtivarPlano(int planoId)
        {
            var usuarioId = GetUserId();
            if (usuarioId != null)
            {
                await _planoAplicacao.AtivarPlano(planoId, (int)usuarioId);
                return NoContent();
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPost("{planoId}/materia")]
        public async Task<IActionResult> AdicionarMateria(
            int planoId,
            [FromBody] AdicionarPlanoMateriaDTO dto)
        {
            await _planoAplicacao.AdicionarMateria(
                planoId,
                dto.MateriaId,
                dto.HorasTotais
            );

            return NoContent();
        }

        [HttpPut("lancar-horas")]
        public async Task<IActionResult> LancarHoras(
            [FromQuery] int planoMateriaId,
            [FromQuery] int horas
        )
        {
            await _planoAplicacao.LancarHoras(planoMateriaId, horas);
            return NoContent();
        }

        [HttpPut("{planoId}/desativar")]
        public async Task<IActionResult> DesativarPlano(int planoId)
        {
            var plano = await _planoAplicacao.Obter(planoId);

            if (plano == null)
                return NotFound("Plano não encontrado!");

            await _planoAplicacao.DesativarPlano(plano);

            return NoContent();
        }


        [HttpGet("horas/comparacao")]
        public async Task<IActionResult> CompararHorasHojeOntem()
        {
            var usuarioId = GetUserId();
            if (usuarioId != null)
            {
                var comparacao = await _planoAplicacao.CompararHorasHojeOntem((int)usuarioId);
                return Ok(comparacao);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpDelete("{planoId}")]
        public async Task<IActionResult> Excluir(int planoId)
        {
            try
            {
                var plano = await _planoAplicacao.Obter(planoId);

                await _planoAplicacao.Excluir(plano);

                return NoContent();
            }
            catch
            {
                return BadRequest("Houve um erro ao fazer a requisição");
            }
        }

        [HttpGet("plano-ativo")]
        public async Task<IActionResult> ObterPlanoAtivo()
        {
            var usuarioId = GetUserId();
            if (usuarioId != null)
            {
                var plano = await _planoAplicacao.ObterPlanoAtivo((int)usuarioId);
                return Ok(plano);
            }
            else
            {
                return Forbid();
            }
        }


    }

}
