using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Planos
{
    public class PlanoNaoEncontradoException : DomainException
    {
        public PlanoNaoEncontradoException()
            : base("Plano de estudo não encontrado.") { }

        public PlanoNaoEncontradoException(int id)
            : base($"Plano de estudo com id {id} não encontrado.") { }
    }
}
