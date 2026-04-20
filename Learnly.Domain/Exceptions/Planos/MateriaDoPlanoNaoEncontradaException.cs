using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Planos
{
    public class MateriaDoPlanoNaoEncontradaException : DomainException
    {
        public MateriaDoPlanoNaoEncontradaException()
            : base("Matéria do plano não encontrada.") { }

        public MateriaDoPlanoNaoEncontradaException(int id)
            : base($"Matéria do plano com id {id} não encontrada.") { }
    }
}
