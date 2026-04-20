using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Planos
{
    public class MateriaNaoEncontradaException : DomainException
    {
        public MateriaNaoEncontradaException()
            : base("Matéria não encontrada.") { }

        public MateriaNaoEncontradaException(int id)
            : base($"Matéria com id {id} não encontrada.") { }
    }
}
