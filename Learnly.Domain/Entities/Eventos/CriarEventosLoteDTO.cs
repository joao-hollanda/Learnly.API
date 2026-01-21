using System.Collections.Generic;

namespace Learnly.Application.DTOs
{
    public class CriarEventosEstudoLoteDto
    {
        public int UsuarioId { get; set; }

        public List<CriarEventoEstudoDto> Eventos { get; set; } = new();
    }
}
