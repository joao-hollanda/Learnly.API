namespace Learnly.Domain.Entities.Redacoes
{
    public class Redacao
    {
        public int RedacaoId { get; set; }
        public int UsuarioId { get; set; }
        public string Tema { get; set; }
        public string Texto { get; set; }

        public int NotaC1 { get; set; }
        public int NotaC2 { get; set; }
        public int NotaC3 { get; set; }
        public int NotaC4 { get; set; }
        public int NotaC5 { get; set; }
        public int NotaFinal { get; set; }

        public string ComentariosJson { get; set; }

        public DateTime Data { get; set; }
    }
}
