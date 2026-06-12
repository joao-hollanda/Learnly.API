using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Learnly.Domain.Entities.Simulados;

namespace Learnly.Api.Models.Simulados.Response
{
    public class SimuladoObter
    {
        public int SimuladoId { get; set; }
        public decimal? NotaFinal { get; set; }
        public DateTime Data { get; set; }

        public List<QuestaoSimuladoDto> Questoes { get; set; }
        public List<RespostaSimuladoDto> Respostas { get; set; }

        public DesempenhoSimulado Desempenho { get; set; }
        public List<MaterialRecomendado> MateriaisRecomendados { get; set; }
    }
}