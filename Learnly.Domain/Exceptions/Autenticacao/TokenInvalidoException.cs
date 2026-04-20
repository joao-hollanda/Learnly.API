using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Autenticacao
{
    public class TokenInvalidoException : DomainException
    {
        public TokenInvalidoException()
            : base("Token inválido ou expirado.") { }
    }
}
