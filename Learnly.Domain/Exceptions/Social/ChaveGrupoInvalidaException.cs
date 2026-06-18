using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Social
{
    public class ChaveGrupoInvalidaException : DomainException
    {
        public ChaveGrupoInvalidaException()
            : base("Código de grupo inválido.") { }
    }
}
