using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Social
{
    public class ConversaNaoEncontradaException : DomainException
    {
        public ConversaNaoEncontradaException()
            : base("Conversa não encontrada.") { }
    }
}
