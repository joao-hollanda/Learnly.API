using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Social
{
    public class AcessoConversaNegadoException : DomainException
    {
        public AcessoConversaNegadoException()
            : base("Você não tem acesso a esta conversa.") { }
    }
}
