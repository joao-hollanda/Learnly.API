using Learnly.Domain.Entities.Planos;

namespace Learnly.Domain.Entities
{
    public class PlanoEstudo
    {
        public int PlanoId { get; set; }

        public string Titulo { get; set; }
        public string Objetivo { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        public int HorasPorSemana { get; set; }
        public bool Ativo { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public ICollection<PlanoMateria> PlanoMaterias { get; set; }
    }
}
