using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learnly.Domain.Entities.Planos
{
    public class Materia
    {
        public int MateriaId { get; set; }

        public string Nome { get; set; }
        public string Cor { get; set; }

        public ICollection<PlanoMateria> PlanoMaterias { get; set; }
    }
}