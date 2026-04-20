using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Usuarios
{
    public class EmailJaCadastradoException : DomainException
    {
        public EmailJaCadastradoException(string email)
            : base($"Já existe um usuário cadastrado com o e-mail '{email}'.") { }
    }
}
