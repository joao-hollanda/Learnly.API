using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;

public class GeradorAgendaService
{
    private readonly Random _random = new();

    private readonly List<TimeSpan> horarios = new()
    {
        new TimeSpan(8, 0, 0),
        new TimeSpan(10, 0, 0),
        new TimeSpan(14, 0, 0),
        new TimeSpan(16, 0, 0),
        new TimeSpan(19, 0, 0)
    };

    public List<EventoEstudo> GerarAgenda(
        PlanoEstudo plano,
        List<PlanoMateria> materias)
    {
        var eventos = new List<EventoEstudo>();
        var dataAtual = DateTime.Today.AddDays(1);

        foreach (var materia in materias)
        {
            int sessoes = materia.HorasTotais;

            for (int i = 0; i < sessoes; i++)
            {
                var horario = horarios[_random.Next(horarios.Count)];

                eventos.Add(new EventoEstudo
                {
                    PlanoId = plano.PlanoId,
                    Titulo = materia.Materia.Nome,
                    Inicio = dataAtual.Add(horario),
                    Fim = dataAtual.Add(horario).AddHours(1)
                });

                dataAtual = ProximoDiaUtil(dataAtual);
            }
        }

        return eventos.OrderBy(_ => _random.Next()).ToList();
    }

    private DateTime ProximoDiaUtil(DateTime data)
    {
        do
        {
            data = data.AddDays(1);
        } while (data.DayOfWeek == DayOfWeek.Saturday ||
                 data.DayOfWeek == DayOfWeek.Sunday);

        return data;
    }
}
