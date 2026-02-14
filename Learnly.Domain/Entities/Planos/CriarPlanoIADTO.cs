public class CriarPlanoIADTO
{
    public string Titulo { get; set; }
    public string Objetivo { get; set; }

    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }

    public int HorasPorSemana { get; set; }
    public int UsuarioId { get; set; }
}