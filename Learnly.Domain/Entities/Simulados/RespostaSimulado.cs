namespace Learnly.Domain.Entities.Simulados
{
    public class RespostaSimulado
    {
        public int RespostaId { get; set; }

        public int SimuladoId { get; set; }
        public Simulado Simulado { get; set; }

        public int QuestaoId { get; set; }
        public Questao Questao { get; set; }

        public int AlternativaId { get; set; }
        public Alternativa Alternativa { get; set; }

        public string? Explicacao { get; set; }
    }
}
