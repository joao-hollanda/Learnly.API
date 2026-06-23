using FluentValidation;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Exceptions.Autenticacao;
using Learnly.Domain.Exceptions.Usuarios;
using Learnly.Repository.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Learnly.Application.Applications
{
    public class UsuarioAplicacao : IUsuarioAplicacao
    {
        readonly IUsuarioRepositorio _usuarioRepositorio;
        readonly IValidator<Usuario> _validator;
        readonly IEmailService _emailService;
        readonly ILoginAplicacao _loginAplicacao;
        readonly IConfiguration _configuration;

        private static readonly TimeSpan ValidadeConfirmacao = TimeSpan.FromHours(24);
        private static readonly TimeSpan ValidadeReset = TimeSpan.FromMinutes(30);

        public UsuarioAplicacao(
            IUsuarioRepositorio usuarioRepositorio,
            IValidator<Usuario> validator,
            IEmailService emailService,
            ILoginAplicacao loginAplicacao,
            IConfiguration configuration)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _validator = validator;
            _emailService = emailService;
            _loginAplicacao = loginAplicacao;
            _configuration = configuration;
        }

        public async Task<int> Criar(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            await _validator.ValidateAsync(usuario, opts =>
            {
                opts.IncludeRuleSets("default", "Criar");
                opts.ThrowOnFailures();
            });

            if (await _usuarioRepositorio.EmailEmUso(usuario.Email))
                throw new EmailJaCadastradoException(usuario.Email);

            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha, 12);
            usuario.EmailConfirmado = false;

            usuario.Id = await _usuarioRepositorio.Criar(usuario);

            await EnviarEmailConfirmacao(usuario);

            return usuario.Id;
        }

        public async Task<Usuario> ConfirmarEmail(string token)
        {
            var principal = _loginAplicacao.ValidarToken(token);
            if (principal == null || principal.FindFirst("tipo")?.Value != "confirmacao")
                throw new TokenInvalidoException();

            if (!int.TryParse(principal.FindFirst("id")?.Value, out var id))
                throw new TokenInvalidoException();

            var usuario = await _usuarioRepositorio.Obter(id, true);
            if (usuario == null)
                throw new TokenInvalidoException();

            if (usuario.EmailConfirmado)
                return usuario;

            usuario.EmailConfirmado = true;
            await _usuarioRepositorio.Atualizar(usuario);

            return usuario;
        }

        public async Task ReenviarConfirmacao(string email)
        {
            var usuario = await _usuarioRepositorio.ObterPorEmail(email);
            if (usuario == null || usuario.EmailConfirmado)
                return;

            await EnviarEmailConfirmacao(usuario);
        }

        public async Task SolicitarRecuperacaoSenha(string email)
        {
            var usuario = await _usuarioRepositorio.ObterPorEmail(email);
            if (usuario == null)
                return;

            var token = _loginAplicacao.GerarTokenAcao(usuario.Id, usuario.Email, "reset", ValidadeReset);
            var link = $"{FrontendUrl()}/redefinir-senha?token={Uri.EscapeDataString(token)}";

            try
            {
                await _emailService.EnviarRecuperacaoSenhaAsync(usuario.Email, usuario.Nome, link);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Falha ao enviar e-mail de recuperação para {usuario.Email}: {ex.Message}");
            }
        }

        public async Task RedefinirSenha(string token, string novaSenha)
        {
            var principal = _loginAplicacao.ValidarToken(token);
            if (principal == null || principal.FindFirst("tipo")?.Value != "reset")
                throw new TokenInvalidoException();

            if (!int.TryParse(principal.FindFirst("id")?.Value, out var id))
                throw new TokenInvalidoException();

            var usuario = await _usuarioRepositorio.Obter(id, true);
            if (usuario == null)
                throw new TokenInvalidoException();

            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(novaSenha, 12);
            await _usuarioRepositorio.Atualizar(usuario);
        }

        private async Task EnviarEmailConfirmacao(Usuario usuario)
        {
            var token = _loginAplicacao.GerarTokenAcao(usuario.Id, usuario.Email, "confirmacao", ValidadeConfirmacao);
            var link = $"{FrontendUrl()}/confirmar-email?token={Uri.EscapeDataString(token)}";

            try
            {
                await _emailService.EnviarConfirmacaoAsync(usuario.Email, usuario.Nome, link);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Falha ao enviar e-mail de confirmação para {usuario.Email}: {ex.Message}");
            }
        }

        private string FrontendUrl() =>
            (_configuration["Email:FrontendUrl"] ?? "https://www.learnly.com.br").TrimEnd('/');

        public async Task Atualizar(Usuario usuario)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuario.Id, true);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException(usuario.Id);

            await _validator.ValidateAndThrowAsync(usuario);

            if (usuarioDominio.Email != usuario.Email)
            {
                if (await _usuarioRepositorio.EmailEmUso(usuario.Email, usuario.Id))
                    throw new EmailJaCadastradoException(usuario.Email);
            }

            usuarioDominio.Nome = usuario.Nome;
            usuarioDominio.Email = usuario.Email;

            await _usuarioRepositorio.Atualizar(usuarioDominio);
        }

        public async Task AtualizarFoto(int usuarioId, string foto)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException(usuarioId);

            usuarioDominio.Foto = foto;

            await _usuarioRepositorio.Atualizar(usuarioDominio);
        }

        public async Task AtualizarSenha(int usuarioId, string senhaAntiga, string novaSenha)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException(usuarioId);

            if (!BCrypt.Net.BCrypt.Verify(senhaAntiga, usuarioDominio.Senha))
                throw new SenhaInvalidaException("A senha atual está incorreta.");

            usuarioDominio.Senha = BCrypt.Net.BCrypt.HashPassword(novaSenha, 12);

            await _usuarioRepositorio.Atualizar(usuarioDominio);
        }

        public async Task<Usuario> Obter(int usuarioId)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException();

            return usuarioDominio;
        }

        public Task Aquecer() => _usuarioRepositorio.Aquecer();

        public async Task<Usuario> ObterPorEmail(string email)
        {
            var usuarioDominio = await _usuarioRepositorio.ObterPorEmail(email);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException();

            return usuarioDominio;
        }
        public async Task<Usuario> ObterPorNome(string nome)
        {
            var usuarioDominio = await _usuarioRepositorio.ObterPorNome(nome);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException();

            return usuarioDominio;
        }

        public async Task Desativar(int usuarioId)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException();

            usuarioDominio.Desativar();

            await _usuarioRepositorio.Atualizar(usuarioDominio);
        }

        public async Task Reativar(int usuarioId)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, false);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException();

            usuarioDominio.Reativar();

            await _usuarioRepositorio.Atualizar(usuarioDominio);
        }

        public async Task<IEnumerable<Usuario>> Listar(bool ativo)
        {
            return await _usuarioRepositorio.Listar(ativo);
        }

    }
}