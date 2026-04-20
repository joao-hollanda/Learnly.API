using Learnly.Api.Models.Usuarios.Request;
using Learnly.Application.Interfaces;
using Learnly.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : BaseController
    {
        private readonly IUsuarioAplicacao _usuarioAplicacao;
        private readonly ILoginAplicacao _loginAplicacao;

        public LoginController(IUsuarioAplicacao usuarioAplicacao, ILoginAplicacao loginAplicacao)
        {
            _loginAplicacao = loginAplicacao;
            _usuarioAplicacao = usuarioAplicacao;
        }

        [HttpGet("AuthCheck")]
        [Authorize]
        public IActionResult AuthCheck() => Success(new { autenticado = true });

        [HttpGet("user")]
        [Authorize]
        public IActionResult GetUser()
        {
            var userId = User.FindFirst("id")?.Value;
            var email = User.FindFirst("email")?.Value;
            var nome = User.FindFirst("nome")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            return Success(new
            {
                id = int.TryParse(userId, out var id) ? id : 0,
                email,
                nome
            });
        }

        [HttpPost]
        public async Task<ActionResult> Login([FromBody] Login loginDTO)
        {
            var usuario = await _usuarioAplicacao.ObterPorEmail(loginDTO.Email);
            var auth = _loginAplicacao.ValidarLogin(usuario, loginDTO.Senha);

            if (!auth)
                return Unauthorized("Usuário ou senha inválido");

            var token = _loginAplicacao.GenerateToken(usuario.Id, usuario.Email, usuario.Nome);
            var isProduction = !Request.Host.Host.Contains("localhost");
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddHours(24)
            };

            Response.Cookies.Append("jwt", token, cookieOptions);
            return Success(new { message = "Login realizado com sucesso" });
        }

        [HttpPost("refresh")]
        [Authorize]
        public IActionResult RefreshToken()
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
            return Success(new { message = "Token renovado com sucesso" });
        }

        [HttpPost("mobile")]
        public async Task<ActionResult> LoginMobile([FromBody] Login loginDTO)
        {
            var usuario = await _usuarioAplicacao.ObterPorEmail(loginDTO.Email);
            if (usuario == null) return Unauthorized("Usuário não registrado");

            var auth = _loginAplicacao.ValidarLogin(usuario, loginDTO.Senha);
            if (!auth) return Unauthorized("Usuário ou senha inválido");

            var accessToken = _loginAplicacao.GenerateToken(
                usuario.Id, usuario.Email, usuario.Nome, TimeSpan.FromHours(1)
            );
            var refreshToken = _loginAplicacao.GenerateToken(
                usuario.Id, usuario.Email, usuario.Nome, TimeSpan.FromDays(30)
            );

            return Ok(new { accessToken, refreshToken });
        }

        [HttpPost("mobile/refresh")]
        public IActionResult RefreshTokenMobile([FromBody] RefreshTokenRequest body)
        {
            var principal = _loginAplicacao.ValidarToken(body.RefreshToken);
            if (principal == null) return Unauthorized("Refresh token inválido");

            var userId = principal.FindFirst("id")?.Value;
            var email = principal.FindFirst("email")?.Value;
            var nome = principal.FindFirst("nome")?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var newAccessToken = _loginAplicacao.GenerateToken(
                int.Parse(userId), email, nome, TimeSpan.FromHours(1)
            );
            var newRefreshToken = _loginAplicacao.GenerateToken(
                int.Parse(userId), email, nome, TimeSpan.FromDays(30)
            );

            return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
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
            return Success(new { message = "Logout realizado com sucesso" });
        }

        [HttpGet("ping")]
        public IActionResult Ping() => Ok("pong");
    }
}
