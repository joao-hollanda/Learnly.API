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

        public bool ValidarLogin(Usuario usuario, string senha)
        {
            if (usuario == null)
                throw new Exception("Usuário inválido");

            if (string.IsNullOrEmpty(senha))
                throw new Exception("Senha é obrigatória");

            if (!BCrypt.Net.BCrypt.Verify(senha, usuario.Senha))
                return false;

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

        public string GenerateToken(int id, string email, string nome, TimeSpan expiracao)
        {
            var claims = new[]
            {
                new Claim("id", id.ToString()),
                new Claim("email", email),
                new Claim("nome", nome),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var privateKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["jwt:secretKey"])
            );
            var credenciais = new SigningCredentials(privateKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["jwt:issuer"],
                audience: _configuration["jwt:audience"],
                claims: claims,
                expires: DateTime.UtcNow.Add(expiracao),
                signingCredentials: credenciais
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidarToken(string token)
        {
            var parametros = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidIssuer = _configuration["jwt:issuer"],
                ValidAudience = _configuration["jwt:audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["jwt:secretKey"])
                )
            };

            try
            {
                return new JwtSecurityTokenHandler().ValidateToken(
                    token, parametros, out _
                );
            }
            catch
            {
                return null;
            }
        }
    }
}