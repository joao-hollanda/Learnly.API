namespace Learnly.Domain.Exceptions.Comuns
{
    public class RegraDeNegocioException : DomainException
    {
        public RegraDeNegocioException(string mensagem) : base(mensagem) { }
    }
}
