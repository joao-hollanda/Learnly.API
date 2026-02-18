using System.Text.RegularExpressions;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Repository.Interfaces;

namespace Learnly.Application.Applications
{
    public class UsuarioAplicacao : IUsuarioAplicacao
    {
        readonly IUsuarioRepositorio _usuarioRepositorio;

        public UsuarioAplicacao(IUsuarioRepositorio usuarioRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
        }

        public async Task<int> Criar(Usuario usuario)
        {
            if (usuario == null)
                throw new Exception("Usuario não pode ser vazio!");

            var user = await _usuarioRepositorio.ObterPorEmail(usuario.Email);

            if (user != null)
                throw new Exception("Já existe um usuário cadastrado com esse email!");

            if (string.IsNullOrEmpty(usuario.Senha))
                throw new Exception("Senha não pode ser vazia!");

            ValidarInformacoesUsuario(usuario);

            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha, 12);

            return await _usuarioRepositorio.Criar(usuario);
        }

        public async Task Atualizar(Usuario usuario)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuario.Id, true);

            if (usuarioDominio == null)
                throw new Exception("Usuario não encontrado!");

            ValidarEmailENome(usuario);

            if (usuarioDominio.Email != usuario.Email)
            {
                var usuarioComEmail = await _usuarioRepositorio.ObterPorEmail(usuario.Email);
                if (usuarioComEmail != null && usuarioComEmail.Id != usuario.Id)
                    throw new Exception("Já existe um usuário cadastrado com esse email!");
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
                throw new Exception("Usuario não encontrado!");

            return usuarioDominio;
        }

        public async Task<Usuario> ObterPorEmail(string email)
        {
            var usuarioDominio = await _usuarioRepositorio.ObterPorEmail(email);

            if (usuarioDominio == null)
                throw new Exception("Usuario não encontrado!");

            return usuarioDominio;
        }
        public async Task<Usuario> ObterPorNome(string nome)
        {
            var usuarioDominio = await _usuarioRepositorio.ObterPorNome(nome);

            if (usuarioDominio == null)
                throw new Exception("Usuario não encontrado!");

            return usuarioDominio;
        }

        public async Task Desativar(int usuarioId)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new Exception("Usuario não encontrado!");

            usuarioDominio.Desativar();

            await _usuarioRepositorio.Atualizar(usuarioDominio);
        }

        public async Task Reativar(int usuarioId)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, false);

            if (usuarioDominio == null)
                throw new Exception("Usuario não encontrado!");

            usuarioDominio.Reativar();

            await _usuarioRepositorio.Atualizar(usuarioDominio);
        }

        public async Task<IEnumerable<Usuario>> Listar(bool ativo)
        {
            return await _usuarioRepositorio.Listar(ativo);
        }

        #region Util
        private static void ValidarInformacoesUsuario(Usuario usuario)
        {
            ValidarEmailENome(usuario);

            if (!string.IsNullOrEmpty(usuario.Senha))
            {
                const string senhaPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s])[^\s]{8,}$";
                if (!Regex.IsMatch(usuario.Senha, senhaPattern))
                    throw new Exception("Senha inválida! A senha deve ter no mínimo 8 caracteres, incluindo pelo menos uma letra minúscula, uma maiúscula, um número e um caractere especial.");
            }
        }

        private static void ValidarEmailENome(Usuario usuario)
        {
            if (string.IsNullOrEmpty(usuario.Nome))
                throw new Exception("Nome não pode ser vazio!");

            if (string.IsNullOrEmpty(usuario.Email))
                throw new Exception("Email não pode ser vazio!");

            const string emailPattern = @"^[A-Za-z0-9.!#$%&'*+/=?^_`{|}~-]+@(?:[A-Za-z0-9-]+\.)+[A-Za-z]{2,}$";
            if (!Regex.IsMatch(usuario.Email, emailPattern))
                throw new Exception("Email inválido! Por favor, insira um email válido.");
        }

        #endregion
    }
}