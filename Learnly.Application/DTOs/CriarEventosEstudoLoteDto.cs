using System.Collections.Generic;

namespace Learnly.Application.DTOs
{
    public class CriarEventosEstudoLoteDto
    {
        public List<CriarEventoEstudoDto> Eventos { get; set; } = new();
    }
}
