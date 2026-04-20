using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Planos
{
    public class MateriaJaAdicionadaException : DomainException
    {
        public MateriaJaAdicionadaException()
            : base("Essa matéria já foi adicionada ao plano.") { }
    }
}
