using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Learnly.Repository.Interfaces;

namespace Learnly.Application.Applications
{
    public class PlanoAplicacao : IPlanoAplicacao
    {
        readonly IPlanoRepositorio _planoRepositorio;
        readonly IUsuarioRepositorio _usuarioRepositorio;
        readonly IMateriaRepositorio _materiaRepositorio;

        public PlanoAplicacao(
            IPlanoRepositorio planoRepositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IMateriaRepositorio materiaRepositorio)
        {
            _planoRepositorio = planoRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _materiaRepositorio = materiaRepositorio;
        }

        public async Task Criar(PlanoEstudo plano)
        {
            if (plano == null)
                throw new ArgumentNullException(nameof(plano));

            if (string.IsNullOrWhiteSpace(plano.Titulo) ||
                string.IsNullOrWhiteSpace(plano.Objetivo))
                throw new InvalidOperationException("É necessário ter um título e um objetivo!");

            await _planoRepositorio.Criar(plano);
        }

        public async Task<PlanoEstudo> Obter(int planoId)
        {
            var plano = await _planoRepositorio.Obter(planoId);

            if (plano == null)
                throw new Exception("Plano não encontrado");

            return plano;
        }

        public async Task<List<PlanoEstudo>> Listar5(int usuarioId)
        {
            var usuario = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuario == null)
                throw new Exception("Usuário não encontrado");

            return await _planoRepositorio.Listar5(usuarioId);
        }

        public async Task Atualizar(PlanoEstudo planoEstudo)
        {
            if (planoEstudo == null)
                throw new ArgumentNullException(nameof(planoEstudo));

            await _planoRepositorio.Atualizar(new List<PlanoEstudo> { planoEstudo });
        }

        public async Task AtivarPlano(int planoId, int usuarioId)
        {
            var planos = await _planoRepositorio.Listar5(usuarioId);

            if (!planos.Any())
                throw new Exception("Usuário não possui planos");

            var planoSelecionado = planos.FirstOrDefault(p => p.PlanoId == planoId);

            if (planoSelecionado == null)
                throw new Exception("Plano não encontrado");

            foreach (var plano in planos)
                plano.Ativo = false;

            planoSelecionado.Ativo = true;

            await _planoRepositorio.Atualizar(planos);
        }

        public async Task AdicionarMateria(int planoId, int materiaId, int horasTotais)
        {
            var plano = await _planoRepositorio.Obter(planoId);

            if (plano == null)
                throw new Exception("Plano não encontrado");

            var materia = await _materiaRepositorio.Obter(materiaId);

            if (materia == null)
                throw new Exception("Matéria não encontrada");

            if (plano.PlanoMaterias.Any(pm => pm.MateriaId == materiaId))
                throw new Exception("Matéria já adicionada ao plano");

            plano.PlanoMaterias.Add(new PlanoMateria
            {
                MateriaId = materiaId,
                HorasTotais = horasTotais,
                HorasConcluidas = 0
            });

            await _planoRepositorio.Atualizar(new List<PlanoEstudo> { plano });
        }

        public async Task LancarHoras(int planoMateriaId, int horas)
        {
            if (horas <= 0)
                throw new Exception("Horas deve ser maior que zero.");

            var planoMateria = await _planoRepositorio.ObterPlanoMateriaPorId(planoMateriaId)
                ?? throw new Exception("Matéria do plano não encontrada.");

            var horasRestantes =
                planoMateria.HorasTotais - planoMateria.HorasConcluidas;

            if (horas > horasRestantes)
                throw new Exception("Horas excedem o total planejado.");

            planoMateria.HorasConcluidas += horas;

            await _planoRepositorio.Salvar();
        }

        public async Task<ResumoGeralDto> GerarResumo(int usuarioId)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new Exception("Usuário não encontrado!");

            return await _planoRepositorio.GerarResumoGeral(usuarioId);
        }
        public async Task GerarAgendaPlano(int planoId)
        {
            var plano = await _planoRepositorio.ObterPlanoPorId(planoId);

            if (plano == null)
                throw new Exception("Plano não encontrado");

            var materias = plano.PlanoMaterias.ToList();

            if (!materias.Any())
                throw new Exception("Plano sem matérias");

            var gerador = new GeradorAgendaService();
            var eventos = gerador.GerarAgenda(plano, materias);

            plano.Agenda.Clear();
            plano.Agenda.AddRange(eventos);

            await _planoRepositorio.Atualizar(new List<PlanoEstudo> {plano});
        }

    }
}