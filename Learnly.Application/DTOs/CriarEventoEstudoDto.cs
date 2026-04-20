namespace Learnly.Application.DTOs
{
    public class CriarEventoEstudoDto
    {
        public string Titulo { get; set; } = null!;

        public DateTime Inicio { get; set; }

        public DateTime Fim { get; set; }
    }
}
