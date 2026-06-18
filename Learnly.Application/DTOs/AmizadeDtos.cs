using System;

namespace Learnly.Application.DTOs
{
    public class AmigoDto
    {
        public int AmizadeId { get; set; }
        public int UsuarioId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
    }

    public class SolicitacaoAmizadeDto
    {
        public int AmizadeId { get; set; }
        public int UsuarioId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public DateTime DataSolicitacao { get; set; }
    }
}
