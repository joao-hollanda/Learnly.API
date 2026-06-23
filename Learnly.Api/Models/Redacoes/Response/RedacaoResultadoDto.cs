using Learnly.Domain.Entities.Redacoes;

namespace Learnly.Api.Models.Redacoes.Response
{
    public class RedacaoResultadoDto
    {
        public int RedacaoId { get; set; }
        public string Tema { get; set; }
        public string Texto { get; set; }
        public int NotaFinal { get; set; }
        public List<CompetenciaNota> Competencias { get; set; }
        public string ComentarioGeral { get; set; }
        public DateTime Data { get; set; }
    }
}
