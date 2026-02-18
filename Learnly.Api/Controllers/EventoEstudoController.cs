using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers
{
    [ApiController]
    [Route("api/eventos")]
    [Authorize]
    public class EventosEstudoController : ControllerBase
    {
        private readonly IEventoEstudoAplicacao _aplicacao;

        public EventosEstudoController(IEventoEstudoAplicacao aplicacao)
        {
            _aplicacao = aplicacao;
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> Listar(int usuarioId)
        {
            var eventos = await _aplicacao.Listar(usuarioId);
            return Ok(eventos);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarEventoRequest request)
        {
            await _aplicacao.Criar(
                request.Titulo,
                request.Inicio,
                request.Fim,
                request.UsuarioId
            );

            return NoContent();
        }

        [HttpDelete("{usuarioId}")]
        public async Task<IActionResult> Remover(int usuarioId)
        {
            await _aplicacao.Remover(usuarioId);
            return NoContent();
        }

        [HttpPost("lote")]
        public async Task<IActionResult> CriarLote([FromBody] CriarEventosEstudoLoteDto request)
        {
            await _aplicacao.CriarEmLote(
                request.UsuarioId,
                request.Eventos
            );

            return NoContent();
        }

    }

    public record CriarEventoRequest(
        string Titulo,
        DateTime Inicio,
        DateTime Fim,
        int UsuarioId
    );
}