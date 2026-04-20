using Learnly.Api.Models.Planos.Request;
using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
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

        public PlanoController(IPlanoAplicacao planoAplicacao)
        {
            _planoAplicacao = planoAplicacao;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarPlanoDTO request)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            var dto = new CriarPlanoIADTO
            {
                Titulo = request.Titulo,
                Objetivo = request.Objetivo,
                DataInicio = request.DataInicio,
                DataFim = request.DataFim,
                HorasPorSemana = request.HorasPorSemana,
                UsuarioId = usuarioId.Value,
                PlanoIa = request.PlanoIa
            };

            var plano = await _planoAplicacao.Criar(dto);
            return Success(plano);
        }

        [HttpGet("{planoId}")]
        public async Task<IActionResult> Obter(int planoId)
        {
            var plano = await _planoAplicacao.Obter(planoId);
            return Success(plano);
        }

        [HttpGet]
        public async Task<IActionResult> Listar5()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            var planos = await _planoAplicacao.Listar5(usuarioId.Value);
            return Success(planos);
        }

        [HttpGet("gerar-resumo")]
        public async Task<IActionResult> ObterResumoGeral()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            return Success(await _planoAplicacao.GerarResumo(usuarioId.Value));
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
            if (usuarioId == null) return Forbid();

            await _planoAplicacao.AtivarPlano(planoId, usuarioId.Value);
            return NoContent();
        }

        [HttpPost("{planoId}/materia")]
        public async Task<IActionResult> AdicionarMateria(int planoId, [FromBody] AdicionarPlanoMateriaDTO dto)
        {
            await _planoAplicacao.AdicionarMateria(planoId, dto.MateriaId, dto.HorasTotais);
            return NoContent();
        }

        [HttpPut("lancar-horas")]
        public async Task<IActionResult> LancarHoras([FromQuery] int planoMateriaId, [FromQuery] int horas)
        {
            await _planoAplicacao.LancarHoras(planoMateriaId, horas);
            return NoContent();
        }

        [HttpPut("{planoId}/desativar")]
        public async Task<IActionResult> DesativarPlano(int planoId)
        {
            await _planoAplicacao.DesativarPlano(planoId);
            return NoContent();
        }

        [HttpGet("horas/comparacao")]
        public async Task<IActionResult> CompararHorasHojeOntem()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            var comparacao = await _planoAplicacao.CompararHorasHojeOntem(usuarioId.Value);
            return Success(comparacao);
        }

        [HttpDelete("{planoId}")]
        public async Task<IActionResult> Excluir(int planoId)
        {
            await _planoAplicacao.Excluir(planoId);
            return NoContent();
        }

        [HttpGet("plano-ativo")]
        public async Task<IActionResult> ObterPlanoAtivo()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            var plano = await _planoAplicacao.ObterPlanoAtivo(usuarioId.Value);
            return Success(plano);
        }
    }
}
