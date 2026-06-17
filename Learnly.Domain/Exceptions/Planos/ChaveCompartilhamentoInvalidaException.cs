using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Planos
{
    public class ChaveCompartilhamentoInvalidaException : DomainException
    {
        public ChaveCompartilhamentoInvalidaException()
            : base("Chave de compartilhamento inválida.") { }
    }
}
