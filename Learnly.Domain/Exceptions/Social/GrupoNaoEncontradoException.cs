using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Social
{
    public class GrupoNaoEncontradoException : DomainException
    {
        public GrupoNaoEncontradoException()
            : base("Grupo não encontrado.") { }

        public GrupoNaoEncontradoException(int id)
            : base($"Grupo com id {id} não encontrado.") { }
    }
}
