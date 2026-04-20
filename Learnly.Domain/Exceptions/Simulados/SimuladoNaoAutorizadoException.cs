using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Simulados
{
    public class SimuladoNaoAutorizadoException : DomainException
    {
        public SimuladoNaoAutorizadoException()
            : base("Este simulado pertence a outro usuário.") { }
    }
}
