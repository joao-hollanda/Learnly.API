using Learnly.Api.Models.Usuarios.Request;
using Learnly.Application.Interfaces;
using Learnly.API.Controllers;
using Learnly.Domain.Exceptions.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
        public async Task<IActionResult> GetUser()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            var usuario = await _usuarioAplicacao.Obter((int)usuarioId);

            return Success(new
            {
                id = usuario.Id,
                nome = usuario.Nome,
                email = usuario.Email,
                foto = usuario.Foto,
                dataCriacao = usuario.DataCriacao,
                emailConfirmado = usuario.EmailConfirmado
            });
        }

        [HttpPost]
        [EnableRateLimiting("login")]
        public async Task<ActionResult> Login([FromBody] Login loginDTO)
        {
            var usuario = await _usuarioAplicacao.ObterPorEmail(loginDTO.Email);
            var auth = _loginAplicacao.ValidarLogin(usuario, loginDTO.Senha);

            if (!auth)
                return Unauthorized("Usuário ou senha inválido");

            if (!usuario.EmailConfirmado)
                throw new EmailNaoConfirmadoException();

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
            return Success(new { id = usuario.Id, nome = usuario.Nome, email = usuario.Email });
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
        [EnableRateLimiting("login")]
        public async Task<ActionResult> LoginMobile([FromBody] Login loginDTO)
        {
            var usuario = await _usuarioAplicacao.ObterPorEmail(loginDTO.Email);
            if (usuario == null) return Unauthorized("Usuário não registrado");

            var auth = _loginAplicacao.ValidarLogin(usuario, loginDTO.Senha);
            if (!auth) return Unauthorized("Usuário ou senha inválido");

            if (!usuario.EmailConfirmado)
                throw new EmailNaoConfirmadoException();

            var accessToken = _loginAplicacao.GenerateToken(
                usuario.Id, usuario.Email, usuario.Nome, TimeSpan.FromHours(1)
            );
            var refreshToken = _loginAplicacao.GenerateToken(
                usuario.Id, usuario.Email, usuario.Nome, TimeSpan.FromDays(30), refreshToken: true
            );

            return Ok(new { accessToken, refreshToken });
        }

        [HttpPost("mobile/refresh")]
        [EnableRateLimiting("login")]
        public IActionResult RefreshTokenMobile([FromBody] RefreshTokenRequest body)
        {
            var principal = _loginAplicacao.ValidarToken(body.RefreshToken);
            if (principal == null) return Unauthorized("Refresh token inválido");

            if (principal.FindFirst("tipo")?.Value != "refresh")
                return Unauthorized("Token informado não é um refresh token");

            var userId = principal.FindFirst("id")?.Value;
            var email = principal.FindFirst("email")?.Value;
            var nome = principal.FindFirst("nome")?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var newAccessToken = _loginAplicacao.GenerateToken(
                int.Parse(userId), email, nome, TimeSpan.FromHours(1)
            );
            var newRefreshToken = _loginAplicacao.GenerateToken(
                int.Parse(userId), email, nome, TimeSpan.FromDays(30), refreshToken: true
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
        [DisableRateLimiting]
        public IActionResult Ping() => Ok("pong");

        [HttpGet("warmup")]
        public async Task<IActionResult> Warmup()
        {
            await _usuarioAplicacao.Aquecer();
            return Ok("warm");
        }

        [HttpPost("confirmar-email")]
        public async Task<IActionResult> ConfirmarEmail([FromBody] ConfirmarEmailRequest body)
        {
            var usuario = await _usuarioAplicacao.ConfirmarEmail(body.Token);

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
            return Success(new { id = usuario.Id, nome = usuario.Nome, email = usuario.Email });
        }

        [HttpGet("descadastrar-emails")]
        [AllowAnonymous]
        public async Task<IActionResult> DescadastrarEmails([FromQuery] string token)
        {
            string mensagem;
            try
            {
                await _usuarioAplicacao.DescadastrarEmails(token);
                mensagem = "Pronto! Você não receberá mais lembretes por e-mail.";
            }
            catch
            {
                mensagem = "Não foi possível processar o link — ele pode ter expirado.";
            }

            var html = $@"<!DOCTYPE html><html lang=""pt-BR""><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width, initial-scale=1""><title>Learnly</title></head>
<body style=""margin:0;background:#f1f5f9;font-family:Arial,Helvetica,sans-serif;color:#1e293b;display:flex;align-items:center;justify-content:center;min-height:100vh"">
  <div style=""max-width:420px;text-align:center;background:#fff;border:1px solid #e2e8f0;border-radius:14px;padding:40px 32px"">
    <div style=""color:#0f1f5c;font-size:22px;font-weight:800;margin-bottom:16px"">Learnly</div>
    <p style=""font-size:15px;line-height:1.6;margin:0"">{mensagem}</p>
  </div>
</body></html>";

            return Content(html, "text/html");
        }

        [HttpPost("reenviar-confirmacao")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> ReenviarConfirmacao([FromBody] EmailRequest body)
        {
            await _usuarioAplicacao.ReenviarConfirmacao(body.Email);
            return Success(new { message = "Se houver uma conta pendente com esse e-mail, enviamos um novo link de confirmação." });
        }

        [HttpPost("esqueci-senha")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> EsqueciSenha([FromBody] EmailRequest body)
        {
            await _usuarioAplicacao.SolicitarRecuperacaoSenha(body.Email);
            return Success(new { message = "Se houver uma conta com esse e-mail, enviamos as instruções de redefinição." });
        }

        [HttpPost("redefinir-senha")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequest body)
        {
            await _usuarioAplicacao.RedefinirSenha(body.Token, body.Senha);
            return Success(new { message = "Senha redefinida com sucesso." });
        }
    }
}
