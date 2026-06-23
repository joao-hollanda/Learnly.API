namespace Learnly.Application.DTOs
{
    public class DashboardDto
    {
        public int HorasEstudadasSemana { get; set; }
        public int HorasSemanaPassada { get; set; }
        public int TotalSimulados { get; set; }
        public int TotalQuestoesRespondidas { get; set; }
        public int TaxaAcertoGeral { get; set; }
        public int SequenciaDias { get; set; }
        public int MelhorSequencia { get; set; }
        public int MetaHorasSemana { get; set; }

        public int TotalRedacoes { get; set; }
        public int MediaRedacao { get; set; }
        public int MelhorRedacao { get; set; }

        public ProgressoPlanoDto ProgressoPlano { get; set; } = new();
        public List<HorasDiaDto> HorasPorDia { get; set; } = new();
        public List<HorasDiaDto> MapaCalor { get; set; } = new();
        public List<DisciplinaDesempenhoDto> DesempenhoPorDisciplina { get; set; } = new();
        public List<EvolucaoSimuladoDto> EvolucaoSimulados { get; set; } = new();
        public List<MateriaProgressoDto> ProgressoPorMateria { get; set; } = new();
        public List<EvolucaoRedacaoDto> EvolucaoRedacoes { get; set; } = new();
        public List<CompetenciaMediaDto> MediaPorCompetencia { get; set; } = new();
    }

    public class ProgressoPlanoDto
    {
        public string Titulo { get; set; }
        public int HorasConcluidas { get; set; }
        public int HorasTotais { get; set; }
        public int Percentual { get; set; }
    }

    public class HorasDiaDto
    {
        public DateTime Data { get; set; }
        public string Dia { get; set; }
        public int Horas { get; set; }
    }

    public class DisciplinaDesempenhoDto
    {
        public string Disciplina { get; set; }
        public int Respondidas { get; set; }
        public int Acertos { get; set; }
        public int PercentualAcerto { get; set; }
    }

    public class EvolucaoSimuladoDto
    {
        public DateTime Data { get; set; }
        public string Rotulo { get; set; }
        public decimal Nota { get; set; }
    }

    public class EvolucaoRedacaoDto
    {
        public DateTime Data { get; set; }
        public string Rotulo { get; set; }
        public int Nota { get; set; }
    }

    public class CompetenciaMediaDto
    {
        public int Numero { get; set; }
        public string Nome { get; set; }
        public int Media { get; set; }
    }

    public class MateriaProgressoDto
    {
        public string Materia { get; set; }
        public string Cor { get; set; }
        public int HorasConcluidas { get; set; }
        public int HorasTotais { get; set; }
    }
}
