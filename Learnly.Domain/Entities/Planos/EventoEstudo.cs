using Learnly.Domain.Entities;

public class EventoEstudo
{
    public int EventoId { get; set; }

    public int PlanoId { get; set; }
    public PlanoEstudo Plano { get; set; }

    public string Titulo { get; set; }

    public DateTime Inicio { get; set; }

    public DateTime Fim { get; set; }
}