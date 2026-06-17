using System.Collections.Generic;

namespace Learnly.Application.DTOs
{
    public class GrupoProgressoDto
    {
        public int GrupoId { get; set; }
        public string Chave { get; set; }
        public List<MembroProgressoDto> Membros { get; set; } = new();
    }

    public class MembroProgressoDto
    {
        public int UsuarioId { get; set; }
        public string Nome { get; set; }
        public int HorasTotais { get; set; }
        public int HorasConcluidas { get; set; }
        public double Percentual { get; set; }
        public bool Eu { get; set; }
    }
}
