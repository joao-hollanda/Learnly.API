using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Social
{
    public class JaMembroDoGrupoException : RegraDeNegocioException
    {
        public JaMembroDoGrupoException()
            : base("Você já faz parte deste grupo.") { }
    }
}
