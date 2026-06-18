using Learnly.Api.Models.Social.Request;
using Learnly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : BaseController
    {
        private readonly IChatAplicacao _chatAplicacao;

        public ChatController(IChatAplicacao chatAplicacao)
        {
            _chatAplicacao = chatAplicacao;
        }

        [HttpGet("conversas")]
        public async Task<IActionResult> ListarConversas()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            return Success(await _chatAplicacao.ListarConversas(usuarioId.Value));
        }

        [HttpGet("conversas/{conversaId}")]
        public async Task<IActionResult> ObterConversa(int conversaId)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            return Success(await _chatAplicacao.ObterConversa(conversaId, usuarioId.Value));
        }

        [HttpGet("conversas/{conversaId}/mensagens")]
        public async Task<IActionResult> ListarMensagens(int conversaId, [FromQuery] int? antesDeId, [FromQuery] int limite = 30)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            return Success(await _chatAplicacao.ListarMensagens(conversaId, usuarioId.Value, antesDeId, limite));
        }

        [HttpPost("direta")]
        public async Task<IActionResult> IniciarDireta([FromBody] IniciarConversaRequest request)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            var conversaId = await _chatAplicacao.ObterOuCriarConversaDireta(usuarioId.Value, request.AmigoId);
            return Success(new { conversaId });
        }

        [HttpPost("conversas/{conversaId}/ler")]
        public async Task<IActionResult> MarcarLida(int conversaId)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            await _chatAplicacao.MarcarLida(conversaId, usuarioId.Value);
            return NoContent();
        }
    }
}
