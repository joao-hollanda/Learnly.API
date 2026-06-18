using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Social
{
    public class SolicitacaoAmizadeInvalidaException : RegraDeNegocioException
    {
        public SolicitacaoAmizadeInvalidaException(string mensagem) : base(mensagem) { }
    }
}
