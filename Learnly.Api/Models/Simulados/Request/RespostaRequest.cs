using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learnly.Api.Models.Simulados.Request
{
    public class RespostaRequest
    {
        public int QuestaoId { get; set; }
        public int AlternativaId { get; set; }
    }
}