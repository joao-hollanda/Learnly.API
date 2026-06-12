using Learnly.API.Controllers;
using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers
{
    [ApiController]
    [Route("api/eventos")]
    [Authorize]
    public class EventosEstudoController : BaseController
    {
        private readonly IEventoEstudoAplicacao _aplicacao;

        public EventosEstudoController(IEventoEstudoAplicacao aplicacao)
        {
            _aplicacao = aplicacao;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            var eventos = await _aplicacao.Listar((int)usuarioId);
            return Success(eventos);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarEventoRequest request)
        {
            await _aplicacao.Criar(
                request.Titulo,
                request.Inicio,
                request.Fim,
                (int)GetUserId()
            );

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Remover()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            await _aplicacao.Remover((int)usuarioId);
            return NoContent();
        }

        [HttpPost("lote")]
        public async Task<IActionResult> CriarLote([FromBody] CriarEventosEstudoLoteDto request)
        {
            await _aplicacao.CriarEmLote(
                (int)GetUserId(),
                request.Eventos
            );

            return NoContent();
        }

    }

    public record CriarEventoRequest(
        string Titulo,
        DateTime Inicio,
        DateTime Fim
    );
}