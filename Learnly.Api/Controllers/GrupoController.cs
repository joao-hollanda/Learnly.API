using Learnly.Api.Models.Social.Request;
using Learnly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GrupoController : BaseController
    {
        private readonly IGrupoAplicacao _grupoAplicacao;

        public GrupoController(IGrupoAplicacao grupoAplicacao)
        {
            _grupoAplicacao = grupoAplicacao;
        }

        [HttpGet]
        public async Task<IActionResult> ListarMeus()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            return Success(await _grupoAplicacao.ListarMeus(usuarioId.Value));
        }

        [HttpGet("{grupoId}")]
        public async Task<IActionResult> Obter(int grupoId)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            return Success(await _grupoAplicacao.Obter(grupoId, usuarioId.Value));
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarGrupoRequest request)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            var grupo = await _grupoAplicacao.Criar(usuarioId.Value, request.Nome, request.Descricao);
            return Success(grupo);
        }

        [HttpPost("entrar")]
        public async Task<IActionResult> Entrar([FromBody] EntrarGrupoRequest request)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            var grupo = await _grupoAplicacao.Entrar(usuarioId.Value, request.Chave);
            return Success(grupo);
        }

        [HttpPost("{grupoId}/sair")]
        public async Task<IActionResult> Sair(int grupoId)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            await _grupoAplicacao.Sair(grupoId, usuarioId.Value);
            return NoContent();
        }
    }
}
