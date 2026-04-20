using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Simulados
{
    public class AlternativaNaoEncontradaException : DomainException
    {
        public AlternativaNaoEncontradaException()
            : base("Alternativa não encontrada.") { }

        public AlternativaNaoEncontradaException(int id)
            : base($"Alternativa com id {id} não encontrada.") { }
    }
}
