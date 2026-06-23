using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Redacoes
{
    public class RedacaoNaoEncontradaException : DomainException
    {
        public RedacaoNaoEncontradaException()
            : base("Redação não encontrada.") { }

        public RedacaoNaoEncontradaException(int id)
            : base($"Redação com id {id} não encontrada.") { }
    }
}
