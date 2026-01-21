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
        readonly IHoraLancadaRepositorio _horaLancadaRepositorio;

        public PlanoAplicacao(
            IPlanoRepositorio planoRepositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IMateriaRepositorio materiaRepositorio,
            IHoraLancadaRepositorio horaLancadaRepositorio)
        {
            _planoRepositorio = planoRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _materiaRepositorio = materiaRepositorio;
            _horaLancadaRepositorio = horaLancadaRepositorio;
        }

        public async Task Criar(PlanoEstudo plano)
        {
            if (plano == null)
                throw new ArgumentNullException(nameof(plano));

            if (string.IsNullOrWhiteSpace(plano.Titulo) ||
                string.IsNullOrWhiteSpace(plano.Objetivo))
                throw new InvalidOperationException("É necessário ter um título e um objetivo!");


            var totalPlanosUsuario =
                await _planoRepositorio.ContarPorUsuario(plano.UsuarioId);

            if (totalPlanosUsuario >= 5)
                throw new InvalidOperationException("Limite máximo de 5 planos atingido.");

            await _planoRepositorio.Criar(plano);
        }

        public async Task<PlanoEstudo> Obter(int planoId)
        {
            var plano = await _planoRepositorio.ObterPlanoPorId(planoId);

            if (plano == null)
                throw new Exception("Plano não encontrado");

            return plano;
        }

        public async Task<List<PlanoEstudo>> Listar5(int usuarioId)
        {
            return await _planoRepositorio.ListarPorUsuario(usuarioId);
        }


        public async Task Atualizar(PlanoEstudo planoEstudo)
        {
            if (planoEstudo == null)
                throw new ArgumentNullException(nameof(planoEstudo));

            await _planoRepositorio.Atualizar(new List<PlanoEstudo> { planoEstudo });
        }

        public async Task AtivarPlano(int planoId, int usuarioId)
        {
            var planos = await _planoRepositorio.ListarPorUsuario(usuarioId);

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

            var lancamento = new HoraLancada
            {
                UsuarioId = planoMateria.Plano.UsuarioId,
                QuantdadeHoras = horas,
                Data = DateTime.UtcNow.Date
            };

            await _horaLancadaRepositorio.LancarHorasAsync(lancamento);

            await _planoRepositorio.Salvar();
        }

        public async Task<ResumoGeralUsuarioDto> GerarResumo(int usuarioId)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuarioDominio == null)
                throw new Exception("Usuário não encontrado!");

            return await _planoRepositorio.GerarResumoGeral(usuarioId);
        }

        public async Task DesativarPlano(PlanoEstudo plano)
        {
            plano.Desativar();
            await _planoRepositorio.Atualizar(new List<PlanoEstudo> { plano });
        }

        public async Task<ComparacaoHorasDto> CompararHorasHojeOntem(int usuarioId)
        {
            var usuario = await _usuarioRepositorio.Obter(usuarioId, true);

            if (usuario == null)
                throw new Exception("Usuário não encontrado");

            var hoje = DateTime.UtcNow.Date;
            var ontem = hoje.AddDays(-1);

            var horasHoje = await _horaLancadaRepositorio
                .SomarHorasPeriodoAsync(usuarioId, hoje, hoje);

            var horasOntem = await _horaLancadaRepositorio
                .SomarHorasPeriodoAsync(usuarioId, ontem, ontem);

            return new ComparacaoHorasDto
            {
                HorasHoje = horasHoje,
                HorasOntem = horasOntem,
                Diferenca = horasHoje - horasOntem
            };
        }

        public async Task Excluir(PlanoEstudo plano)
        {
            await _planoRepositorio.Excluir(plano);
        }
        public async Task<PlanoEstudo> ObterPlanoAtivo(int usuarioId)
        {
            return await _planoRepositorio.ObterPlanoAtivo(usuarioId);
        }

    }

}