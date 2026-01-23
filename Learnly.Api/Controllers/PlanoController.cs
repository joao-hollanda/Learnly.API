using Learnly.Api.Models.Planos.Request;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlanoController : ControllerBase
    {
        private readonly IPlanoAplicacao _planoAplicacao;

        public PlanoController(IPlanoAplicacao planoAplicacao)
        {
            _planoAplicacao = planoAplicacao;
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarPlanoDTO dto)
        {
            var plano = new PlanoEstudo
            {
                Titulo = dto.Titulo,
                Objetivo = dto.Objetivo,
                UsuarioId = dto.UsuarioId,

                DataInicio = DateTime.SpecifyKind(dto.DataInicio, DateTimeKind.Utc),
                DataFim = DateTime.SpecifyKind(dto.DataFim, DateTimeKind.Utc),

                HorasPorSemana = 0,
                Ativo = false
            };

            await _planoAplicacao.Criar(plano);
            return Ok(plano);
        }


        [HttpGet("{planoId}")]
        public async Task<IActionResult> Obter(int planoId)
        {
            var plano = await _planoAplicacao.Obter(planoId);
            return Ok(plano);
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> Listar5(int usuarioId)
        {
            var planos = await _planoAplicacao.Listar5(usuarioId);
            return Ok(planos);
        }

        [HttpGet("gerar-resumo/{usuarioId}")]
        public async Task<ActionResult<ResumoGeralDto>> ObterResumoGeral(int usuarioId)
        {
            return Ok(await _planoAplicacao.GerarResumo(usuarioId));
        }


        [HttpPut]
        public async Task<IActionResult> Atualizar([FromBody] PlanoEstudo plano)
        {
            await _planoAplicacao.Atualizar(plano);
            return NoContent();
        }

        [HttpPut("{planoId}/ativar")]
        public async Task<IActionResult> AtivarPlano(int planoId, [FromQuery] int usuarioId)
        {
            await _planoAplicacao.AtivarPlano(planoId, usuarioId);
            return NoContent();
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


        [HttpGet("horas/comparacao/{usuarioId}")]
        public async Task<IActionResult> CompararHorasHojeOntem(int usuarioId)
        {
            var comparacao = await _planoAplicacao.CompararHorasHojeOntem(usuarioId);
            return Ok(comparacao);
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("plano-ativo/{usuarioId}")]
        public async Task<IActionResult> ObterPlanoAtivo(int usuarioId)
        {
            var plano = await _planoAplicacao.ObterPlanoAtivo(usuarioId);
            return Ok(plano);
        }


    }

}
