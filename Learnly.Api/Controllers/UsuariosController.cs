using System.Threading.Tasks;
using Learnly.Api.Models.Usuarios.Request;
using Learnly.Api.Models.Usuarios.Response;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Learnly.API.Controllers;

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

        [HttpPost]
        [Route("Criar")]
        [AllowAnonymous]
        public async Task<ActionResult> Criar([FromBody] UsuarioCriar usuarioCriar)
        {
            try
            {
                var usuarioDominio = new Usuario()
                {
                    Nome = usuarioCriar.Nome,
                    Email = usuarioCriar.Email,
                    Senha = usuarioCriar.Senha
                };

                var usuarioId = await _usuarioAplicacao.Criar(usuarioDominio);

                return Ok(usuarioId);
            }
            catch
            {
                return BadRequest("Houve um erro ao fazer a requisição");

            }
        }

        // [HttpGet]
        // [Route("{usuarioId}")]
        // public async Task<IActionResult> Obter([FromRoute] int usuarioId)
        // {
        //     try
        //     {
        //         var usuarioDominio = await _usuarioAplicacao.Obter(usuarioId);

        //         var usuarioResposta = new UsuarioResponse()
        //         {
        //             Id = usuarioDominio.Id,
        //             Nome = usuarioDominio.Nome,
        //             Email = usuarioDominio.Email,
        //         };

        //         return Ok(usuarioResposta);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }

        // [HttpGet]
        // public async Task<IActionResult> Listar([FromQuery] bool ativo)
        // {

        //     try
        //     {
        //         var usuariosDominio = await _usuarioAplicacao.Listar(ativo);

        //         var usuarios = usuariosDominio.Select(u => new UsuarioResponse()
        //         {
        //             Id = u.Id,
        //             Nome = u.Nome,
        //             Email = u.Email,
        //         }).ToList();

        //         return Ok(usuarios);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }

        [HttpPut]
        public async Task<IActionResult> Atualizar([FromBody] UsuarioAtualizar usuario)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            try
            {
                var usuarioDominio = new Usuario()
                {
                    Id = (int)usuarioId,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                };

                await _usuarioAplicacao.Atualizar(usuarioDominio);

                return Ok();
            }
            catch
            {
                return BadRequest("Houve um erro ao fazer a requisição");
            }
        }
        // [HttpPut]
        // [Route("AlterarSenha/{Id}")]
        // public async Task<IActionResult> AlterarSenha([FromRoute] int Id, [FromBody] UsuarioAtualizarSenha usuario)
        // {
        //     try
        //     {
        //         await _usuarioAplicacao.AlterarSenha(Id, usuario.SenhaAntiga, usuario.Senha);

        //         return Ok("Senha alterada com sucesso!");
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }


        [HttpPut]
        [Route("Reativar/{usuarioId}")]
        public async Task<IActionResult> Reativar()
        {
            try
            {
                var usuarioId = GetUserId();
                if (usuarioId == null)
                    return Forbid();

                await _usuarioAplicacao.Reativar((int)usuarioId);

                return Ok();
            }
            catch
            {
                return BadRequest("Houve um erro ao fazer a requisição");
            }
        }

        [HttpDelete("Desativar/{usuarioId}")]
        public async Task<IActionResult> Desativar()
        {
            try
            {
                var usuarioId = GetUserId();
                if (usuarioId == null)
                    return Forbid();
                await _usuarioAplicacao.Desativar((int)usuarioId);

                return Ok();
            }
            catch
            {
                return BadRequest("Houve um erro ao fazer a requisição");
            }
        }
    }
}