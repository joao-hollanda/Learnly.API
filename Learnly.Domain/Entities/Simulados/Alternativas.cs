namespace Learnly.Domain.Entities.Simulados
{
    public class Alternativa
    {
        public int AlternativaId { get; set; }
        public int QuestaoId { get; set; }
        public Questao Questao { get; set; }
        public string Letra { get; set; }
        public string? Texto { get; set; }
        public string? Arquivo { get; set; }
        public bool Correta { get; set; }
    }
}