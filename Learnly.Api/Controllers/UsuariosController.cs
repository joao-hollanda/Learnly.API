using System.Threading.Tasks;
using Learnly.Api.Models.Usuarios.Request;
using Learnly.Api.Models.Usuarios.Response;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
        [Route("{Id}")]
        public async Task<IActionResult> Atualizar([FromRoute] int Id, [FromBody] UsuarioAtualizar usuario)
        {
            try
            {
                var usuarioDominio = new Usuario()
                {
                    Id = Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                };

                await _usuarioAplicacao.Atualizar(usuarioDominio);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
        public async Task<IActionResult> Reativar([FromRoute] int usuarioId)
        {
            try
            {
                await _usuarioAplicacao.Reativar(usuarioId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("Desativar/{usuarioId}")]
        public async Task<IActionResult> Desativar([FromRoute] int usuarioId)
        {
            try
            {
                await _usuarioAplicacao.Desativar(usuarioId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}