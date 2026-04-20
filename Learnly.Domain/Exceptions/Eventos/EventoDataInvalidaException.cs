using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Eventos
{
    public class EventoDataInvalidaException : DomainException
    {
        public EventoDataInvalidaException()
            : base("A data de fim do evento deve ser posterior à data de início.") { }
    }
}
