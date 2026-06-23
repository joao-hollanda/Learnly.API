using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Usuarios
{
    public class EmailNaoConfirmadoException : DomainException
    {
        public EmailNaoConfirmadoException()
            : base("Confirme seu e-mail para acessar sua conta. Reenvie o link se precisar.") { }
    }
}
