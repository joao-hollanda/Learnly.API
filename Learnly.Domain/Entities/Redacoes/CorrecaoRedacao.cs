namespace Learnly.Domain.Entities.Redacoes
{
    public class CorrecaoRedacao
    {
        public List<CompetenciaNota> Competencias { get; set; } = new();
        public string ComentarioGeral { get; set; }
    }

    public class CompetenciaNota
    {
        public int Numero { get; set; }
        public int Nota { get; set; }
        public string Comentario { get; set; }
    }
}
