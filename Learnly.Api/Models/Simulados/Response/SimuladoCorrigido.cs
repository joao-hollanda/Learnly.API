using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Learnly.Domain.Entities.Simulados;

namespace Learnly.Api.Models.Simulados.Response
{
    public class SimuladoCorrigido
    {
        public int SimuladoId { get; set; }
        public decimal Nota { get; set; }
        public DesempenhoSimulado Desempenho { get; set; }
        public List<MaterialRecomendado> MateriaisRecomendados { get; set; }
    }
}