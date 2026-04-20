using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Simulados
{
    public class RespostasNaoInformadasException : DomainException
    {
        public RespostasNaoInformadasException()
            : base("Nenhuma resposta foi enviada para o simulado.") { }
    }
}
