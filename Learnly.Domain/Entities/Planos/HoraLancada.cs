using Learnly.Domain.Entities;

public class HoraLancada
{
    public int HoraLancadaId { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }
    public int QuantdadeHoras { get; set; }
    public DateTime Data { get; set; }
}