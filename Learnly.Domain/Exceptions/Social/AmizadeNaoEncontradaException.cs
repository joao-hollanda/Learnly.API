using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Social
{
    public class AmizadeNaoEncontradaException : DomainException
    {
        public AmizadeNaoEncontradaException()
            : base("Solicitação de amizade não encontrada.") { }
    }
}
