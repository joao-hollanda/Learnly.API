using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Simulados
{
    public class SimuladoNaoEncontradoException : DomainException
    {
        public SimuladoNaoEncontradoException()
            : base("Simulado não encontrado.") { }

        public SimuladoNaoEncontradoException(int id)
            : base($"Simulado com id {id} não encontrado.") { }
    }
}
