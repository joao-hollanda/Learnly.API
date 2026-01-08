public class QuestaoSimuladoDto
{
    public int QuestaoId { get; set; }
    public string Titulo { get; set; }
    public string Disciplina { get; set; }
    public string Contexto { get; set; }
    public string Arquivos { get; set; }
    public string IntroducaoAlternativa { get; set; }

    public List<AlternativaDto> Alternativas { get; set; }
}