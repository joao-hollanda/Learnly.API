using FluentValidation;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Exceptions.Usuarios;
using Learnly.Repository.Interfaces;

namespace Learnly.Application.Applications
{
    public class UsuarioAplicacao : IUsuarioAplicacao
    {
        readonly IUsuarioRepositorio _usuarioRepositorio;
        readonly IValidator<Usuario> _validator;

        public UsuarioAplicacao(IUsuarioRepositorio usuarioRepositorio, IValidator<Usuario> validator)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _validator = validator;
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

            var user = await _usuarioRepositorio.ObterPorEmail(usuario.Email);
            if (user != null)
                throw new EmailJaCadastradoException(usuario.Email);

            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha, 12);

            return await _usuarioRepositorio.Criar(usuario);
        }

        public async Task Atualizar(Usuario usuario)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuario.Id, true);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException(usuario.Id);

            await _validator.ValidateAndThrowAsync(usuario);

            if (usuarioDominio.Email != usuario.Email)
            {
                var usuarioComEmail = await _usuarioRepositorio.ObterPorEmail(usuario.Email);
                if (usuarioComEmail != null && usuarioComEmail.Id != usuario.Id)
                    throw new EmailJaCadastradoException(usuario.Email);
            }

            usuarioDominio.Nome = usuario.Nome;
            usuarioDominio.Email = usuario.Email;

            await _usuarioRepositorio.Atualizar(usuarioDominio);
        }

        // public async Task AlterarSenha(int usuarioId, string senhaAntiga, string novaSenha)
        // {
        //     var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

        //     if (usuarioDominio == null)
        //         throw new Exception("Usuario não encontrado!");

        //     if (!BCrypt.Net.BCrypt.Verify(senhaAntiga, usuarioDominio.Senha))
        //         throw new Exception("Senha antiga incorreta!");

        //     // Validação de senha com regex
        //     const string senhaPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s])[^\s]{8,}$";
        //     if (!Regex.IsMatch(novaSenha, senhaPattern))
        //         throw new Exception("Senha inválida! A senha deve ter no mínimo 8 caracteres, incluindo pelo menos uma letra minúscula, uma maiúscula, um número e um caractere especial.");

        //     usuarioDominio.Senha = BCrypt.Net.BCrypt.HashPassword(novaSenha, 12);

        //     await _usuarioRepositorio.Atualizar(usuarioDominio);
        // }


        public async Task<Usuario> Obter(int usuarioId)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new UsuarioNaoEncontradoException();

            return usuarioDominio;
        }

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