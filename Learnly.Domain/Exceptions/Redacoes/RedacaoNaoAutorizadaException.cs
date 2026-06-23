using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Redacoes
{
    public class RedacaoNaoAutorizadaException : DomainException
    {
        public RedacaoNaoAutorizadaException()
            : base("Esta redação não pertence ao usuário.") { }
    }
}
