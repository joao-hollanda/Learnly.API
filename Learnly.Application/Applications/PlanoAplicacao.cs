using FluentValidation;
using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Learnly.Domain.Exceptions.Planos;
using Learnly.Domain.Exceptions.Usuarios;
using Learnly.Repository.Interfaces;
using Learnly.Services.Interfaces;

namespace Learnly.Application.Applications
{
    public class PlanoAplicacao : IPlanoAplicacao
    {
        readonly IPlanoRepositorio _planoRepositorio;
        readonly IUsuarioRepositorio _usuarioRepositorio;
        readonly IMateriaRepositorio _materiaRepositorio;
        readonly IHoraLancadaRepositorio _horaLancadaRepositorio;
        readonly IIAService _iaService;
        readonly IValidator<PlanoEstudo> _validator;

        public PlanoAplicacao(
            IPlanoRepositorio planoRepositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IMateriaRepositorio materiaRepositorio,
            IHoraLancadaRepositorio horaLancadaRepositorio,
            IIAService iaService,
            IValidator<PlanoEstudo> validator)
        {
            _planoRepositorio = planoRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _materiaRepositorio = materiaRepositorio;
            _horaLancadaRepositorio = horaLancadaRepositorio;
            _iaService = iaService;
            _validator = validator;
        }

        public async Task<PlanoEstudo> Criar(CriarPlanoIADTO dto)
        {
            PlanoEstudo plano;

            if (dto.PlanoIa)
            {
                var planoBase = new PlanoEstudo
                {
                    Titulo = dto.Titulo,
                    Objetivo = dto.Objetivo,
                    DataInicio = dto.DataInicio,
                    DataFim = dto.DataFim,
                    HorasPorSemana = dto.HorasPorSemana,
                    UsuarioId = dto.UsuarioId
                };
                plano = await _iaService.GerarPlanoIA(planoBase);
                plano.Titulo = dto.Titulo;
                plano.Objetivo = dto.Objetivo;
                plano.DataInicio = dto.DataInicio;
                plano.DataFim = dto.DataFim;
                plano.HorasPorSemana = dto.HorasPorSemana;
                plano.UsuarioId = dto.UsuarioId;
            }
            else
            {
                plano = new PlanoEstudo
                {
                    Titulo = dto.Titulo,
                    Objetivo = dto.Objetivo,
                    UsuarioId = dto.UsuarioId,
                    DataInicio = DateTime.SpecifyKind(dto.DataInicio, DateTimeKind.Utc),
                    DataFim = DateTime.SpecifyKind(dto.DataFim, DateTimeKind.Utc),
                    HorasPorSemana = 0,
                    Ativo = false
                };
            }

            await _validator.ValidateAndThrowAsync(plano);

            var totalPlanosUsuario = await _planoRepositorio.ContarPorUsuario(plano.UsuarioId);
            if (totalPlanosUsuario >= 5)
                throw new LimitePlanosAtingidoException();

            plano.PlanoMaterias ??= new List<PlanoMateria>();

            foreach (var planoMateria in plano.PlanoMaterias)
            {
                if (planoMateria.Materia == null) continue;

                var nomeMateria = planoMateria.Materia.Nome?.Trim();
                if (string.IsNullOrEmpty(nomeMateria)) continue;

                var materiaExistente = await _materiaRepositorio.ObterPorNome(nomeMateria);

                if (materiaExistente != null)
                {
                    planoMateria.Materia = materiaExistente;
                    planoMateria.MateriaId = materiaExistente.MateriaId;
                }
                else
                {
                    planoMateria.Materia = new Materia
                    {
                        Nome = nomeMateria,
                        Cor = planoMateria.Materia.Cor,
                        GeradaPorIA = planoMateria.Materia.GeradaPorIA
                    };
                }
            }

            await _planoRepositorio.Criar(plano);
            return plano;
        }

        public async Task<PlanoEstudo> Obter(int planoId)
        {
            var plano = await _planoRepositorio.ObterPlanoPorId(planoId);

            if (plano == null)
                throw new PlanoNaoEncontradoException(planoId);

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
                throw new PlanoNaoEncontradoException();

            var planoSelecionado = planos.FirstOrDefault(p => p.PlanoId == planoId)
                ?? throw new PlanoNaoEncontradoException(planoId);

            foreach (var plano in planos)
                plano.Ativo = false;

            planoSelecionado.Ativo = true;

            await _planoRepositorio.Atualizar(planos);
        }

        public async Task AdicionarMateria(int planoId, int materiaId, int horasTotais)
        {
            var plano = await _planoRepositorio.Obter(planoId)
                ?? throw new PlanoNaoEncontradoException(planoId);

            var materia = await _materiaRepositorio.Obter(materiaId)
                ?? throw new MateriaNaoEncontradaException(materiaId);

            if (plano.PlanoMaterias.Any(pm => pm.MateriaId == materiaId))
                throw new MateriaJaAdicionadaException();

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
                throw new ArgumentException("Horas deve ser maior que zero.");

            var planoMateria = await _planoRepositorio.ObterPlanoMateriaPorId(planoMateriaId)
                ?? throw new MateriaDoPlanoNaoEncontradaException(planoMateriaId);

            var horasRestantes = planoMateria.HorasTotais - planoMateria.HorasConcluidas;

            if (horas > horasRestantes)
                throw new HorasExcedemTotalException(horasRestantes);

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

        public async Task<ResumoGeralDto> GerarResumo(int usuarioId)
        {
            var usuarioDominio = await _usuarioRepositorio.Obter(usuarioId, true)
                ?? throw new UsuarioNaoEncontradoException(usuarioId);

            var (horasTotais, horasConcluidas) = await _planoRepositorio.GerarResumoGeral(usuarioId);
            return new ResumoGeralDto { HorasTotais = horasTotais, HorasConcluidas = horasConcluidas };
        }

        public async Task DesativarPlano(int planoId)
        {
            var plano = await Obter(planoId);
            plano.Desativar();
            await _planoRepositorio.Atualizar(new List<PlanoEstudo> { plano });
        }

        public async Task<ComparacaoHorasDto> CompararHorasHojeOntem(int usuarioId)
        {
            var usuario = await _usuarioRepositorio.Obter(usuarioId, true)
                ?? throw new UsuarioNaoEncontradoException(usuarioId);

            var hoje = DateTime.UtcNow.Date;
            var ontem = hoje.AddDays(-1);

            var horasHoje = await _horaLancadaRepositorio.SomarHorasPeriodoAsync(usuarioId, hoje, hoje);
            var horasOntem = await _horaLancadaRepositorio.SomarHorasPeriodoAsync(usuarioId, ontem, ontem);

            return new ComparacaoHorasDto
            {
                HorasHoje = horasHoje,
                HorasOntem = horasOntem,
                Diferenca = horasHoje - horasOntem
            };
        }

        public async Task Excluir(int planoId)
        {
            var plano = await Obter(planoId);
            await _planoRepositorio.Excluir(plano);
        }

        public async Task<PlanoEstudo> ObterPlanoAtivo(int usuarioId)
        {
            return await _planoRepositorio.ObterPlanoAtivo(usuarioId);
        }
    }
}
