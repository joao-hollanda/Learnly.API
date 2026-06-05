using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities.Planos;
using Learnly.Domain.Entities.Simulados;
using Learnly.Domain.Exceptions.Usuarios;
using Learnly.Repository.Interfaces;

namespace Learnly.Application.Applications
{
    public class DesempenhoAplicacao : IDesempenhoAplicacao
    {
        static readonly string[] NomesDias = { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };

        readonly ISimuladoRepositorio _simuladoRepositorio;
        readonly IPlanoRepositorio _planoRepositorio;
        readonly IHoraLancadaRepositorio _horaLancadaRepositorio;
        readonly IUsuarioRepositorio _usuarioRepositorio;

        public DesempenhoAplicacao(
            ISimuladoRepositorio simuladoRepositorio,
            IPlanoRepositorio planoRepositorio,
            IHoraLancadaRepositorio horaLancadaRepositorio,
            IUsuarioRepositorio usuarioRepositorio)
        {
            _simuladoRepositorio = simuladoRepositorio;
            _planoRepositorio = planoRepositorio;
            _horaLancadaRepositorio = horaLancadaRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
        }

        public async Task<DashboardDto> ObterDashboard(int usuarioId)
        {
            _ = await _usuarioRepositorio.Obter(usuarioId, true)
                ?? throw new UsuarioNaoEncontradoException(usuarioId);

            var hoje = DateTime.UtcNow.Date;
            var inicioSemana = hoje.AddDays(-6);
            var inicioSemanaPassada = hoje.AddDays(-13);
            var fimSemanaPassada = hoje.AddDays(-7);

            var lancamentos = await _horaLancadaRepositorio.ListarPeriodoAsync(usuarioId, inicioSemana, hoje);

            var horasPorDia = new List<HorasDiaDto>();
            for (var i = 6; i >= 0; i--)
            {
                var dia = hoje.AddDays(-i);
                horasPorDia.Add(new HorasDiaDto
                {
                    Data = dia,
                    Dia = NomesDias[(int)dia.DayOfWeek],
                    Horas = lancamentos.Where(l => l.Data.Date == dia).Sum(l => l.QuantdadeHoras)
                });
            }

            var horasSemana = horasPorDia.Sum(h => h.Horas);
            var horasSemanaPassada = await _horaLancadaRepositorio.SomarHorasPeriodoAsync(usuarioId, inicioSemanaPassada, fimSemanaPassada);

            var datas = await _horaLancadaRepositorio.ListarDatasComLancamento(usuarioId);
            var (sequenciaAtual, melhorSequencia) = CalcularSequencia(datas, hoje);

            var respostas = await _simuladoRepositorio.ListarRespostasComQuestao(usuarioId);
            var totalQuestoes = respostas.Count;
            var totalAcertos = respostas.Count(EhCorreta);

            var desempenhoPorDisciplina = respostas
                .Where(r => r.Questao != null)
                .GroupBy(r => r.Questao.Disciplina)
                .Select(g => new DisciplinaDesempenhoDto
                {
                    Disciplina = g.Key,
                    Respondidas = g.Count(),
                    Acertos = g.Count(EhCorreta),
                    PercentualAcerto = Percentual(g.Count(EhCorreta), g.Count())
                })
                .OrderByDescending(d => d.Respondidas)
                .ToList();

            var notas = await _simuladoRepositorio.ListarNotas(usuarioId);
            var evolucao = notas.Select(s => new EvolucaoSimuladoDto
            {
                Data = s.Data,
                Rotulo = s.Data.ToString("dd/MM"),
                Nota = s.NotaFinal
            }).ToList();

            var planoAtivo = await _planoRepositorio.ObterPlanoAtivo(usuarioId);
            var progressoPlano = new ProgressoPlanoDto();
            var progressoPorMateria = new List<MateriaProgressoDto>();
            var metaHorasSemana = 0;

            if (planoAtivo != null)
            {
                var materias = planoAtivo.PlanoMaterias ?? new List<PlanoMateria>();
                var horasTotais = materias.Sum(m => m.HorasTotais);
                var horasConcluidas = materias.Sum(m => m.HorasConcluidas);

                progressoPlano = new ProgressoPlanoDto
                {
                    Titulo = planoAtivo.Titulo,
                    HorasTotais = horasTotais,
                    HorasConcluidas = horasConcluidas,
                    Percentual = Percentual(horasConcluidas, horasTotais)
                };

                progressoPorMateria = materias
                    .Select(m => new MateriaProgressoDto
                    {
                        Materia = m.Materia?.Nome ?? "—",
                        Cor = m.Materia?.Cor,
                        HorasConcluidas = m.HorasConcluidas,
                        HorasTotais = m.HorasTotais
                    })
                    .OrderByDescending(m => m.HorasConcluidas)
                    .ToList();

                metaHorasSemana = planoAtivo.HorasPorSemana;
            }

            return new DashboardDto
            {
                HorasEstudadasSemana = horasSemana,
                HorasSemanaPassada = horasSemanaPassada,
                TotalSimulados = await _simuladoRepositorio.ContarTotal(usuarioId),
                TotalQuestoesRespondidas = totalQuestoes,
                TaxaAcertoGeral = Percentual(totalAcertos, totalQuestoes),
                SequenciaDias = sequenciaAtual,
                MelhorSequencia = melhorSequencia,
                MetaHorasSemana = metaHorasSemana,
                ProgressoPlano = progressoPlano,
                HorasPorDia = horasPorDia,
                DesempenhoPorDisciplina = desempenhoPorDisciplina,
                EvolucaoSimulados = evolucao,
                ProgressoPorMateria = progressoPorMateria
            };
        }

        static bool EhCorreta(RespostaSimulado r) =>
            r.Alternativa != null &&
            r.Questao != null &&
            r.Alternativa.Letra == r.Questao.AlternativaCorreta;

        static int Percentual(int parte, int total) =>
            total > 0 ? (int)Math.Round((double)parte / total * 100) : 0;

        static (int Atual, int Melhor) CalcularSequencia(List<DateTime> datas, DateTime hoje)
        {
            var dias = datas.Select(d => d.Date).Distinct().OrderByDescending(d => d).ToList();
            if (dias.Count == 0) return (0, 0);

            var atual = 0;
            if (dias[0] == hoje || dias[0] == hoje.AddDays(-1))
            {
                atual = 1;
                var referencia = dias[0];
                for (var i = 1; i < dias.Count; i++)
                {
                    if (dias[i] == referencia.AddDays(-1))
                    {
                        atual++;
                        referencia = dias[i];
                    }
                    else break;
                }
            }

            var crescente = dias.OrderBy(d => d).ToList();
            var melhor = 1;
            var corrente = 1;
            for (var i = 1; i < crescente.Count; i++)
            {
                corrente = crescente[i] == crescente[i - 1].AddDays(1) ? corrente + 1 : 1;
                if (corrente > melhor) melhor = corrente;
            }

            return (atual, melhor);
        }
    }
}
