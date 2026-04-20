namespace Learnly.Domain.Exceptions.Comuns
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string mensagem) : base(mensagem) { }

        protected DomainException(string mensagem, Exception inner) : base(mensagem, inner) { }
    }
}
