using System.Security.Cryptography;
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
        readonly IValidator<PlanoEstudo> _validator;

        public PlanoAplicacao(
            IPlanoRepositorio planoRepositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IMateriaRepositorio materiaRepositorio,
            IHoraLancadaRepositorio horaLancadaRepositorio,
            IValidator<PlanoEstudo> validator)
        {
            _planoRepositorio = planoRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _materiaRepositorio = materiaRepositorio;
            _horaLancadaRepositorio = horaLancadaRepositorio;
            _validator = validator;
        }

        // Cria e persiste um PlanoEstudo já montado
        public async Task<PlanoEstudo> Criar(CriarPlanoIADTO dto)
        {
            var plano = new PlanoEstudo
            {
                Titulo = dto.Titulo,
                Objetivo = dto.Objetivo,
                UsuarioId = dto.UsuarioId,
                DataInicio = DateTime.SpecifyKind(dto.DataInicio, DateTimeKind.Utc),
                DataFim = DateTime.SpecifyKind(dto.DataFim, DateTimeKind.Utc),
                HorasPorSemana = dto.HorasPorSemana,
                Ativo = false
            };

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

        // Persiste um PlanoEstudo já gerado pela IA
        public async Task<PlanoEstudo> CriarDaIA(PlanoEstudo plano)
        {
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
                    planoMateria.Materia = null;
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
        public async Task<PlanoEstudo> ObterPlanoAtivoComTracking(int usuarioId)
        {
            return await _planoRepositorio.ObterPlanoAtivoComTracking(usuarioId);
        }

        public async Task<GrupoEstudo> Compartilhar(int planoId, int usuarioId)
        {
            var plano = await _planoRepositorio.ObterPlanoPorId(planoId)
                ?? throw new PlanoNaoEncontradoException(planoId);

            if (plano.UsuarioId != usuarioId)
                throw new PlanoNaoEncontradoException(planoId);

            if (plano.GrupoId != null)
            {
                var grupoExistente = await _planoRepositorio.ObterGrupoPorId(plano.GrupoId.Value);
                if (grupoExistente != null)
                    return grupoExistente;
            }

            var grupo = new GrupoEstudo
            {
                Chave = await GerarChaveUnica(),
                CriadorId = usuarioId
            };

            await _planoRepositorio.CriarGrupo(grupo);

            plano.GrupoId = grupo.GrupoId;
            await _planoRepositorio.Atualizar(new List<PlanoEstudo> { plano });

            return grupo;
        }

        public async Task<PlanoEstudo> Resgatar(string chave, int usuarioId)
        {
            var grupo = await _planoRepositorio.ObterGrupoPorChave(chave?.Trim().ToUpperInvariant())
                ?? throw new ChaveCompartilhamentoInvalidaException();

            if (grupo.CriadorId == usuarioId)
                throw new PlanoJaResgatadoException();

            var jaResgatado = await _planoRepositorio.ObterPlanoDoGrupoPorUsuario(grupo.GrupoId, usuarioId);
            if (jaResgatado != null)
                throw new PlanoJaResgatadoException();

            var totalPlanosUsuario = await _planoRepositorio.ContarPorUsuario(usuarioId);
            if (totalPlanosUsuario >= 5)
                throw new LimitePlanosAtingidoException();

            var modelo = await _planoRepositorio.ObterPlanoDoGrupoPorUsuario(grupo.GrupoId, grupo.CriadorId)
                ?? throw new PlanoNaoEncontradoException();

            var clone = new PlanoEstudo
            {
                Titulo = modelo.Titulo,
                Objetivo = modelo.Objetivo,
                DataInicio = modelo.DataInicio,
                DataFim = modelo.DataFim,
                HorasPorSemana = modelo.HorasPorSemana,
                UsuarioId = usuarioId,
                Ativo = false,
                GrupoId = grupo.GrupoId,
                PlanoMaterias = modelo.PlanoMaterias.Select(pm => new PlanoMateria
                {
                    MateriaId = pm.MateriaId,
                    HorasTotais = pm.HorasTotais,
                    Topicos = pm.Topicos?.ToList(),
                    HorasConcluidas = 0
                }).ToList()
            };

            await _planoRepositorio.Criar(clone);
            return clone;
        }

        public async Task<GrupoProgressoDto> ObterGrupo(int grupoId, int usuarioId)
        {
            var planos = await _planoRepositorio.ListarPlanosDoGrupo(grupoId);

            if (!planos.Any(p => p.UsuarioId == usuarioId))
                throw new PlanoNaoEncontradoException();

            var grupo = await _planoRepositorio.ObterGrupoPorId(grupoId)
                ?? throw new PlanoNaoEncontradoException();

            var membros = planos
                .Select(p =>
                {
                    var horasTotais = p.PlanoMaterias.Sum(pm => pm.HorasTotais);
                    var horasConcluidas = p.PlanoMaterias.Sum(pm => pm.HorasConcluidas);

                    return new MembroProgressoDto
                    {
                        UsuarioId = p.UsuarioId,
                        Nome = p.Usuario?.Nome,
                        HorasTotais = horasTotais,
                        HorasConcluidas = horasConcluidas,
                        Percentual = horasTotais > 0
                            ? Math.Round((double)horasConcluidas / horasTotais * 100, 1)
                            : 0,
                        Eu = p.UsuarioId == usuarioId
                    };
                })
                .OrderByDescending(m => m.Percentual)
                .ToList();

            return new GrupoProgressoDto
            {
                GrupoId = grupo.GrupoId,
                Chave = grupo.CriadorId == usuarioId ? grupo.Chave : null,
                Membros = membros
            };
        }

        private async Task<string> GerarChaveUnica()
        {
            string chave;
            do
            {
                chave = GerarChave();
            } while (await _planoRepositorio.ChaveExiste(chave));

            return chave;
        }

        private static string GerarChave()
        {
            const string alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var caracteres = new char[8];

            for (int i = 0; i < caracteres.Length; i++)
                caracteres[i] = alfabeto[RandomNumberGenerator.GetInt32(alfabeto.Length)];

            return new string(caracteres);
        }
    }
}