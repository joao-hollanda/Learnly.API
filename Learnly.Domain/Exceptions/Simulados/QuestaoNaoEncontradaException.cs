using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Simulados
{
    public class QuestaoNaoEncontradaException : DomainException
    {
        public QuestaoNaoEncontradaException()
            : base("Questão não encontrada.") { }

        public QuestaoNaoEncontradaException(int id)
            : base($"Questão com id {id} não encontrada.") { }
    }
}
