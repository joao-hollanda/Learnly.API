using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Planos
{
    public class PlanoJaResgatadoException : RegraDeNegocioException
    {
        public PlanoJaResgatadoException()
            : base("Você já resgatou este plano compartilhado.") { }
    }
}
