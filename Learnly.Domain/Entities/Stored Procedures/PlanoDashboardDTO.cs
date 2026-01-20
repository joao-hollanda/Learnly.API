public class PlanoDashboardDto
{
    public int PlanoId { get; set; }
    public int UsuarioId { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
}
