using System.IdentityModel.Tokens.Jwt;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Repository.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Learnly.Application.Applications
{
    public class LoginAplicacao : ILoginAplicacao
    {
        readonly IUsuarioRepositorio _usuarioRepositorio;
        readonly IConfiguration _configuration;

        public LoginAplicacao(IUsuarioRepositorio usuarioRepositorio, IConfiguration configuration)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _configuration = configuration;
        }
        public async Task<bool> ValidarLogin(string email, string senha)
        {
            var usuarioDominio = await _usuarioRepositorio.ObterPorEmail(email);

            if (usuarioDominio == null)
                throw new Exception("Usuario não encontrado!");

            if (string.IsNullOrEmpty(senha))
                throw new Exception("Senha é obrigatória!");

            if (string.IsNullOrEmpty(email))
                throw new Exception("Email é obrigatório!");

            if (!BCrypt.Net.BCrypt.Verify(senha, usuarioDominio.Senha))
                throw new Exception("Senha incorreta!");

            return true;
        }

        public string GenerateToken(int id, string email, string nome)
        {
            var claims = new[]
            {
            new Claim("id", id.ToString()),
            new Claim("email", email),
            new Claim("nome", nome),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var privateKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:secretKey"]));

            var credenciais = new SigningCredentials(privateKey, SecurityAlgorithms.HmacSha256);

            var expiracao = DateTime.UtcNow.AddHours(24);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["jwt:issuer"],
                audience: _configuration["jwt:audience"],
                claims: claims,
                expires: expiracao,
                signingCredentials: credenciais
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}