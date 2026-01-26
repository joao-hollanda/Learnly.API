using System.Threading.Tasks;
using Learnly.Api.Models.Usuarios.Request;
using Learnly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUsuarioAplicacao _usuarioAplicacao;
        private readonly ILoginAplicacao _loginAplicacao;

        public LoginController(IUsuarioAplicacao usuarioAplicacao, ILoginAplicacao loginAplicacao)
        {
            _loginAplicacao = loginAplicacao;
            _usuarioAplicacao = usuarioAplicacao;
        }

        [HttpGet]
        [Route("AuthCheck")]
        [Authorize]
        public IActionResult AuthCheck()
        {
            return Ok(new { autenticado = true });
        }

        [HttpPost]
        public async Task<ActionResult> Login([FromBody] Login loginDTO)
        {
            try
            {
                var usuario = await _usuarioAplicacao.ObterPorEmail(loginDTO.Email);

                if (usuario == null)
                    return Unauthorized("Usuário não registrado");

                var auth = await _loginAplicacao.ValidarLogin(loginDTO.Email, loginDTO.Senha);

                if (!auth)
                {
                    return Unauthorized("Usuário ou senha inválido");
                }

                var token = _loginAplicacao.GenerateToken(usuario.Id, usuario.Email, usuario.Nome);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(16)
                };

                Response.Cookies.Append("jwt", token, cookieOptions);

                var resposta = new TokenUsuario
                {
                    Token = token
                };

                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}