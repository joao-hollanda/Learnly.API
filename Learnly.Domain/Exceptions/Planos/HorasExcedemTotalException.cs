using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Planos
{
    public class HorasExcedemTotalException : DomainException
    {
        public HorasExcedemTotalException()
            : base("A quantidade de horas informada excede o total planejado para essa matéria.") { }

        public HorasExcedemTotalException(int horasRestantes)
            : base($"A quantidade de horas informada excede o total planejado. Restam apenas {horasRestantes} hora(s).") { }
    }
}
