using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Eventos
{
    public class EventoNaoEncontradoException : DomainException
    {
        public EventoNaoEncontradoException()
            : base("Evento de estudo não encontrado.") { }

        public EventoNaoEncontradoException(int id)
            : base($"Evento de estudo com id {id} não encontrado.") { }
    }
}
