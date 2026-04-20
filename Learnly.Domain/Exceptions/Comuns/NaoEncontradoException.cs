namespace Learnly.Domain.Exceptions.Comuns
{
    public class NaoEncontradoException : DomainException
    {
        public NaoEncontradoException(string recurso)
            : base($"{recurso} não encontrado(a).") { }

        public NaoEncontradoException(string recurso, int id)
            : base($"{recurso} com id {id} não encontrado(a).") { }
    }
}
