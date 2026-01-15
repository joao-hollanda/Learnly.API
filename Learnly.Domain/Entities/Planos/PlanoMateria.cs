using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learnly.Domain.Entities.Planos
{
    public class PlanoMateria
    {
        public int PlanoMateriaId { get; set; }

        public int PlanoId { get; set; }
        public PlanoEstudo Plano { get; set; }

        public int MateriaId { get; set; }
        public Materia Materia { get; set; }

        public int HorasTotais { get; set; }
        public int HorasConcluidas { get; set; }
    }
}