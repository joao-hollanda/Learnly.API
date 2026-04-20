using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Usuarios
{
    public class UsuarioNaoEncontradoException : DomainException
    {
        public UsuarioNaoEncontradoException()
            : base("Usuário não encontrado.") { }

        public UsuarioNaoEncontradoException(int id)
            : base($"Usuário com id {id} não encontrado.") { }

        public UsuarioNaoEncontradoException(string email)
            : base($"Nenhum usuário encontrado com o e-mail '{email}'.") { }
    }
}
