using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Planos
{
    public class LimitePlanosAtingidoException : DomainException
    {
        public LimitePlanosAtingidoException(int limite = 5)
            : base($"Limite máximo de {limite} planos de estudo atingido.") { }
    }
}
