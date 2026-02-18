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

        [HttpGet("user")]
        [Authorize]
        public IActionResult GetUser()
        {
            var userId = User.FindFirst("id")?.Value;
            var email = User.FindFirst("email")?.Value;
            var nome = User.FindFirst("nome")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            return Ok(new
            {
                id = int.Parse(userId),
                email = email,
                nome = nome
            });
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

                var isProduction = !Request.Host.Host.Contains("localhost");
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isProduction || Request.IsHttps,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(24)
                };

                Response.Cookies.Append("jwt", token, cookieOptions);

                return Ok(new { message = "Login realizado com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("refresh")]
        [Authorize]
        public IActionResult RefreshToken()
        {
            try
            {
                var userId = User.FindFirst("id")?.Value;
                var email = User.FindFirst("email")?.Value;
                var nome = User.FindFirst("nome")?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(nome))
                    return Unauthorized("Token inválido");

                var newToken = _loginAplicacao.GenerateToken(int.Parse(userId), email, nome);

                var isProduction = !Request.Host.Host.Contains("localhost");
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isProduction || Request.IsHttps,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(24)
                };

                Response.Cookies.Append("jwt", newToken, cookieOptions);

                return Ok(new { message = "Token renovado com sucesso" });
            }
            catch (Exception ex)
            {
                return Unauthorized("Erro ao renovar token");
            }
        }
        
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var isProduction = !Request.Host.Host.Contains("localhost");
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction || Request.IsHttps,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            
            Response.Cookies.Append("jwt", "", cookieOptions);
            return Ok(new { message = "Logout realizado com sucesso" });
        }
        
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}