using Learnly.Api.Models.Social.Request;
using Learnly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AmizadeController : BaseController
    {
        private readonly IAmizadeAplicacao _amizadeAplicacao;

        public AmizadeController(IAmizadeAplicacao amizadeAplicacao)
        {
            _amizadeAplicacao = amizadeAplicacao;
        }

        [HttpGet]
        public async Task<IActionResult> ListarAmigos()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            return Success(await _amizadeAplicacao.ListarAmigos(usuarioId.Value));
        }

        [HttpGet("pendentes")]
        public async Task<IActionResult> ListarPendentes()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            return Success(await _amizadeAplicacao.ListarPendentesRecebidas(usuarioId.Value));
        }

        [HttpGet("enviadas")]
        public async Task<IActionResult> ListarEnviadas()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            return Success(await _amizadeAplicacao.ListarPendentesEnviadas(usuarioId.Value));
        }

        [HttpPost("solicitar")]
        public async Task<IActionResult> Solicitar([FromBody] EnviarSolicitacaoRequest request)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            var dto = await _amizadeAplicacao.EnviarSolicitacao(usuarioId.Value, request.EmailOuNome);
            return Success(dto);
        }

        [HttpPost("{amizadeId}/aceitar")]
        public async Task<IActionResult> Aceitar(int amizadeId)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            await _amizadeAplicacao.Aceitar(amizadeId, usuarioId.Value);
            return NoContent();
        }

        [HttpPost("{amizadeId}/recusar")]
        public async Task<IActionResult> Recusar(int amizadeId)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            await _amizadeAplicacao.Recusar(amizadeId, usuarioId.Value);
            return NoContent();
        }

        [HttpDelete("{amizadeId}")]
        public async Task<IActionResult> Remover(int amizadeId)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            await _amizadeAplicacao.Remover(amizadeId, usuarioId.Value);
            return NoContent();
        }
    }
}
