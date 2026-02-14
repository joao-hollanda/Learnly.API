using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learnly.Api.Models.Planos.Request
{
    public class CriarPlanoDTO
    {
        public string Titulo { get; set; }
        public string Objetivo { get; set; }
        public int UsuarioId { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int HorasPorSemana { get; set; } = 0;
        public bool PlanoIa {get;set;} = false;
    }
}