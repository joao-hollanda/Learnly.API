namespace Learnly.Domain.Entities
{
    public class EventoEstudo
    {
        public int EventoEstudoId { get; set; }

        public string Titulo { get; set; } = null!;

        public DateTime Inicio { get; set; }

        public DateTime Fim { get; set; }

        public int UsuarioId { get; set; }
    }
}
