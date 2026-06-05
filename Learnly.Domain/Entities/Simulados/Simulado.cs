using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Simulados;

public class Simulado
{
    public int SimuladoId { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }
    
    public decimal NotaFinal { get; set; }
    public DateTime Data { get; set; }

    public List<SimuladoQuestao> Questoes { get; set; } = new();
    public List<RespostaSimulado> Respostas { get; set; } = new();

    public DesempenhoSimulado Desempenho { get; set; }
    public List<MaterialRecomendado> MateriaisRecomendados { get; set; } = new();
}
