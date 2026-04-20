using Learnly.Api.Models.Usuarios.Request;
using Learnly.Api.Models.Usuarios.Response;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : BaseController
    {
        private readonly IUsuarioAplicacao _usuarioAplicacao;

        public UsuariosController(IUsuarioAplicacao usuarioAplicacao)
        {
            _usuarioAplicacao = usuarioAplicacao;
        }

        [HttpPost("Criar")]
        [AllowAnonymous]
        public async Task<ActionResult> Criar([FromBody] UsuarioCriar usuarioCriar)
        {
            var usuarioDominio = new Usuario
            {
                Nome = usuarioCriar.Nome,
                Email = usuarioCriar.Email,
                Senha = usuarioCriar.Senha
            };

            var usuarioId = await _usuarioAplicacao.Criar(usuarioDominio);
            return Success(new { id = usuarioId });
        }

        [HttpPut]
        public async Task<IActionResult> Atualizar([FromBody] UsuarioAtualizar usuario)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            var usuarioDominio = new Usuario
            {
                Id = (int)usuarioId,
                Nome = usuario.Nome,
                Email = usuario.Email,
            };

            await _usuarioAplicacao.Atualizar(usuarioDominio);
            return Success();
        }

        [HttpPut("Reativar/{usuarioId}")]
        public async Task<IActionResult> Reativar([FromRoute] int usuarioId)
        {
            await _usuarioAplicacao.Reativar(usuarioId);
            return Success();
        }

        [HttpDelete("Desativar/{usuarioId}")]
        public async Task<IActionResult> Desativar([FromRoute] int usuarioId)
        {
            await _usuarioAplicacao.Desativar(usuarioId);
            return Success();
        }
    }
}
