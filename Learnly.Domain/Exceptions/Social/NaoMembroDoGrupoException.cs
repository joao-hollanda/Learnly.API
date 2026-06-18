using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Social
{
    public class NaoMembroDoGrupoException : DomainException
    {
        public NaoMembroDoGrupoException()
            : base("Você não faz parte deste grupo.") { }
    }
}
