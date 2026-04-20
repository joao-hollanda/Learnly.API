using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Autenticacao
{
    public class CredenciaisInvalidasException : DomainException
    {
        public CredenciaisInvalidasException()
            : base("E-mail ou senha incorretos.") { }
    }
}
