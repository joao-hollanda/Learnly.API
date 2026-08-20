using FluentValidation;
using FluentValidation.Results;
using Learnly.Application.Applications;
using Learnly.Application.DTOs;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Learnly.Domain.Exceptions.Planos;
using Learnly.Domain.Exceptions.Usuarios;
using Learnly.Repository.Interfaces;
using Moq;

namespace Learnly.Tests
{
    public class PlanoAplicacaoTests
    {
        private readonly Mock<IPlanoRepositorio> _planoRepo = new();
        private readonly Mock<IUsuarioRepositorio> _usuarioRepo = new();
        private readonly Mock<IMateriaRepositorio> _materiaRepo = new();
        private readonly Mock<IHoraLancadaRepositorio> _horaRepo = new();
        private readonly Mock<IValidator<PlanoEstudo>> _validator = new();

        private PlanoAplicacao CriarSut()
        {
            _validator
                .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _validator
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<PlanoEstudo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            return new(
                _planoRepo.Object,
                _usuarioRepo.Object,
                _materiaRepo.Object,
                _horaRepo.Object,
                _validator.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public async Task LancarHoras_HorasInvalidas_LancaArgumentException(int horas)
        {
            var sut = CriarSut();

            await Assert.ThrowsAsync<ArgumentException>(() => sut.LancarHoras(1, horas));
        }

        [Fact]
        public async Task LancarHoras_MateriaDoPlanoInexistente_LancaNaoEncontrada()
        {
            _planoRepo.Setup(r => r.ObterPlanoMateriaPorId(1)).ReturnsAsync((PlanoMateria)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<MateriaDoPlanoNaoEncontradaException>(() => sut.LancarHoras(1, 2));
        }

        [Fact]
        public async Task LancarHoras_ExcedeHorasRestantes_LancaExcedemTotal()
        {
            _planoRepo.Setup(r => r.ObterPlanoMateriaPorId(1)).ReturnsAsync(new PlanoMateria
            {
                HorasTotais = 10,
                HorasConcluidas = 8,
                Plano = new PlanoEstudo { UsuarioId = 1 }
            });
            var sut = CriarSut();

            await Assert.ThrowsAsync<HorasExcedemTotalException>(() => sut.LancarHoras(1, 3));
        }

        [Fact]
        public async Task LancarHoras_Valido_IncrementaERegistraLancamento()
        {
            var planoMateria = new PlanoMateria
            {
                HorasTotais = 10,
                HorasConcluidas = 8,
                Plano = new PlanoEstudo { UsuarioId = 7 }
            };
            _planoRepo.Setup(r => r.ObterPlanoMateriaPorId(1)).ReturnsAsync(planoMateria);
            var sut = CriarSut();

            await sut.LancarHoras(1, 2);

            Assert.Equal(10, planoMateria.HorasConcluidas);
            _horaRepo.Verify(r => r.LancarHorasAsync(It.Is<HoraLancada>(h =>
                h.UsuarioId == 7 &&
                h.QuantdadeHoras == 2 &&
                h.Data == DateTime.UtcNow.Date)), Times.Once);
            _planoRepo.Verify(r => r.Salvar(), Times.Once);
        }

        [Fact]
        public async Task AdicionarMateria_JaNoPlano_LancaJaAdicionada()
        {
            _planoRepo.Setup(r => r.Obter(1)).ReturnsAsync(new PlanoEstudo
            {
                PlanoMaterias = new List<PlanoMateria> { new() { MateriaId = 2 } }
            });
            _materiaRepo.Setup(r => r.Obter(2)).ReturnsAsync(new Materia { MateriaId = 2 });
            var sut = CriarSut();

            await Assert.ThrowsAsync<MateriaJaAdicionadaException>(() => sut.AdicionarMateria(1, 2, 40));
        }

        [Fact]
        public async Task AdicionarMateria_Valida_AdicionaComHorasZeradas()
        {
            var plano = new PlanoEstudo { PlanoMaterias = new List<PlanoMateria>() };
            _planoRepo.Setup(r => r.Obter(1)).ReturnsAsync(plano);
            _materiaRepo.Setup(r => r.Obter(2)).ReturnsAsync(new Materia { MateriaId = 2 });
            var sut = CriarSut();

            await sut.AdicionarMateria(1, 2, 40);

            var adicionada = Assert.Single(plano.PlanoMaterias);
            Assert.Equal(2, adicionada.MateriaId);
            Assert.Equal(40, adicionada.HorasTotais);
            Assert.Equal(0, adicionada.HorasConcluidas);
            _planoRepo.Verify(r => r.Atualizar(It.IsAny<List<PlanoEstudo>>()), Times.Once);
        }

        [Fact]
        public async Task AtivarPlano_DesativaOsDemaisDoUsuario()
        {
            var planos = new List<PlanoEstudo>
            {
                new() { PlanoId = 1, Ativo = true },
                new() { PlanoId = 2, Ativo = false },
                new() { PlanoId = 3, Ativo = false }
            };
            _planoRepo.Setup(r => r.ListarPorUsuario(1)).ReturnsAsync(planos);
            var sut = CriarSut();

            await sut.AtivarPlano(2, 1);

            Assert.False(planos[0].Ativo);
            Assert.True(planos[1].Ativo);
            Assert.False(planos[2].Ativo);
            _planoRepo.Verify(r => r.Atualizar(planos), Times.Once);
        }

        [Fact]
        public async Task AtivarPlano_PlanoDeOutroUsuario_LancaNaoEncontrado()
        {
            _planoRepo.Setup(r => r.ListarPorUsuario(1)).ReturnsAsync(new List<PlanoEstudo>
            {
                new() { PlanoId = 1 }
            });
            var sut = CriarSut();

            await Assert.ThrowsAsync<PlanoNaoEncontradoException>(() => sut.AtivarPlano(99, 1));
        }

        [Fact]
        public async Task Criar_UsuarioComCincoPlanos_LancaLimiteAtingido()
        {
            _planoRepo.Setup(r => r.ContarPorUsuario(1)).ReturnsAsync(5);
            var sut = CriarSut();

            var dto = new CriarPlanoIADTO
            {
                Titulo = "Sexto plano",
                Objetivo = "Não deve passar",
                UsuarioId = 1,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddMonths(3),
                HorasPorSemana = 10
            };

            await Assert.ThrowsAsync<LimitePlanosAtingidoException>(() => sut.Criar(dto));
        }

        [Fact]
        public async Task Criar_Valido_PersisteInativoComDatasEmUtc()
        {
            _planoRepo.Setup(r => r.ContarPorUsuario(1)).ReturnsAsync(2);
            var sut = CriarSut();

            var plano = await sut.Criar(new CriarPlanoIADTO
            {
                Titulo = "Plano",
                Objetivo = "ENEM",
                UsuarioId = 1,
                DataInicio = new DateTime(2026, 1, 1),
                DataFim = new DateTime(2026, 4, 1),
                HorasPorSemana = 10
            });

            Assert.False(plano.Ativo);
            Assert.Equal(DateTimeKind.Utc, plano.DataInicio.Kind);
            Assert.Equal(DateTimeKind.Utc, plano.DataFim.Kind);
            Assert.Empty(plano.PlanoMaterias);
            _planoRepo.Verify(r => r.Criar(plano), Times.Once);
        }

        [Fact]
        public async Task CriarDaIA_LimiteAtingido_LancaLimitePlanos()
        {
            _planoRepo.Setup(r => r.ContarPorUsuario(1)).ReturnsAsync(5);
            var sut = CriarSut();

            await Assert.ThrowsAsync<LimitePlanosAtingidoException>(
                () => sut.CriarDaIA(new PlanoEstudo { UsuarioId = 1 }));
        }

        [Fact]
        public async Task CriarDaIA_MateriaJaCadastrada_ReaproveitaOIdExistente()
        {
            _planoRepo.Setup(r => r.ContarPorUsuario(1)).ReturnsAsync(0);
            _materiaRepo.Setup(r => r.ObterPorNome("Biologia"))
                .ReturnsAsync(new Materia { MateriaId = 9, Nome = "Biologia" });
            var plano = new PlanoEstudo
            {
                UsuarioId = 1,
                PlanoMaterias = new List<PlanoMateria>
                {
                    new() { Materia = new Materia { Nome = "  Biologia  " } }
                }
            };
            var sut = CriarSut();

            await sut.CriarDaIA(plano);

            var planoMateria = Assert.Single(plano.PlanoMaterias);
            Assert.Null(planoMateria.Materia);
            Assert.Equal(9, planoMateria.MateriaId);
        }

        [Fact]
        public async Task CriarDaIA_MateriaNova_MantemCorEOrigemDaIA()
        {
            _planoRepo.Setup(r => r.ContarPorUsuario(1)).ReturnsAsync(0);
            _materiaRepo.Setup(r => r.ObterPorNome(It.IsAny<string>())).ReturnsAsync((Materia)null);
            var plano = new PlanoEstudo
            {
                UsuarioId = 1,
                PlanoMaterias = new List<PlanoMateria>
                {
                    new() { Materia = new Materia { Nome = "Astronomia", Cor = "#444444", GeradaPorIA = true } }
                }
            };
            var sut = CriarSut();

            await sut.CriarDaIA(plano);

            var materia = Assert.Single(plano.PlanoMaterias).Materia;
            Assert.Equal("Astronomia", materia.Nome);
            Assert.Equal("#444444", materia.Cor);
            Assert.True(materia.GeradaPorIA);
        }

        [Fact]
        public async Task CriarDaIA_MateriaSemNome_NaoConsultaRepositorio()
        {
            _planoRepo.Setup(r => r.ContarPorUsuario(1)).ReturnsAsync(0);
            var plano = new PlanoEstudo
            {
                UsuarioId = 1,
                PlanoMaterias = new List<PlanoMateria>
                {
                    new() { Materia = null },
                    new() { Materia = new Materia { Nome = "   " } }
                }
            };
            var sut = CriarSut();

            await sut.CriarDaIA(plano);

            _materiaRepo.Verify(r => r.ObterPorNome(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Obter_PlanoInexistente_LancaNaoEncontrado()
        {
            _planoRepo.Setup(r => r.ObterPlanoPorId(1)).ReturnsAsync((PlanoEstudo)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<PlanoNaoEncontradoException>(() => sut.Obter(1));
        }

        [Fact]
        public async Task Listar5_DelegaParaRepositorio()
        {
            var planos = new List<PlanoEstudo> { new() { PlanoId = 1 } };
            _planoRepo.Setup(r => r.ListarPorUsuario(1)).ReturnsAsync(planos);
            var sut = CriarSut();

            Assert.Same(planos, await sut.Listar5(1));
        }

        [Fact]
        public async Task Atualizar_PlanoNulo_LancaArgumentNull()
        {
            var sut = CriarSut();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Atualizar(null));
        }

        [Fact]
        public async Task Atualizar_Plano_EnviaListaComOPlano()
        {
            var plano = new PlanoEstudo { PlanoId = 1 };
            var sut = CriarSut();

            await sut.Atualizar(plano);

            _planoRepo.Verify(r => r.Atualizar(It.Is<List<PlanoEstudo>>(l => l.Single() == plano)), Times.Once);
        }

        [Fact]
        public async Task AtivarPlano_UsuarioSemPlanos_LancaNaoEncontrado()
        {
            _planoRepo.Setup(r => r.ListarPorUsuario(1)).ReturnsAsync(new List<PlanoEstudo>());
            var sut = CriarSut();

            await Assert.ThrowsAsync<PlanoNaoEncontradoException>(() => sut.AtivarPlano(1, 1));
        }

        [Fact]
        public async Task AdicionarMateria_PlanoInexistente_LancaNaoEncontrado()
        {
            _planoRepo.Setup(r => r.Obter(1)).ReturnsAsync((PlanoEstudo)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<PlanoNaoEncontradoException>(() => sut.AdicionarMateria(1, 2, 40));
        }

        [Fact]
        public async Task AdicionarMateria_MateriaInexistente_LancaNaoEncontrada()
        {
            _planoRepo.Setup(r => r.Obter(1)).ReturnsAsync(new PlanoEstudo
            {
                PlanoMaterias = new List<PlanoMateria>()
            });
            _materiaRepo.Setup(r => r.Obter(2)).ReturnsAsync((Materia)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<MateriaNaoEncontradaException>(() => sut.AdicionarMateria(1, 2, 40));
        }

        [Fact]
        public async Task GerarResumo_UsuarioInexistente_LancaUsuarioNaoEncontrado()
        {
            _usuarioRepo.Setup(r => r.Obter(1, true)).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(() => sut.GerarResumo(1));
        }

        [Fact]
        public async Task GerarResumo_UsuarioExistente_RetornaHorasDoRepositorio()
        {
            _usuarioRepo.Setup(r => r.Obter(1, true)).ReturnsAsync(new Usuario { Id = 1 });
            _planoRepo.Setup(r => r.GerarResumoGeral(1)).ReturnsAsync((120, 45));
            var sut = CriarSut();

            var resumo = await sut.GerarResumo(1);

            Assert.Equal(120, resumo.HorasTotais);
            Assert.Equal(45, resumo.HorasConcluidas);
        }

        [Fact]
        public async Task DesativarPlano_PlanoExistente_DesativaEAtualiza()
        {
            var plano = new PlanoEstudo { PlanoId = 1 };
            _planoRepo.Setup(r => r.ObterPlanoPorId(1)).ReturnsAsync(plano);
            var sut = CriarSut();

            await sut.DesativarPlano(1);

            Assert.False(plano.Ativo);
            _planoRepo.Verify(r => r.Atualizar(It.Is<List<PlanoEstudo>>(l => l.Single() == plano)), Times.Once);
        }

        [Fact]
        public async Task CompararHorasHojeOntem_UsuarioInexistente_LancaUsuarioNaoEncontrado()
        {
            _usuarioRepo.Setup(r => r.Obter(1, true)).ReturnsAsync((Usuario)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(() => sut.CompararHorasHojeOntem(1));
        }

        [Fact]
        public async Task CompararHorasHojeOntem_CalculaDiferencaEntreOsDias()
        {
            var hoje = DateTime.UtcNow.Date;
            _usuarioRepo.Setup(r => r.Obter(1, true)).ReturnsAsync(new Usuario { Id = 1 });
            _horaRepo.Setup(r => r.SomarHorasPeriodoAsync(1, hoje, hoje)).ReturnsAsync(5);
            _horaRepo.Setup(r => r.SomarHorasPeriodoAsync(1, hoje.AddDays(-1), hoje.AddDays(-1))).ReturnsAsync(2);
            var sut = CriarSut();

            var comparacao = await sut.CompararHorasHojeOntem(1);

            Assert.Equal(5, comparacao.HorasHoje);
            Assert.Equal(2, comparacao.HorasOntem);
            Assert.Equal(3, comparacao.Diferenca);
        }

        [Fact]
        public async Task Excluir_PlanoExistente_DelegaParaRepositorio()
        {
            var plano = new PlanoEstudo { PlanoId = 1 };
            _planoRepo.Setup(r => r.ObterPlanoPorId(1)).ReturnsAsync(plano);
            var sut = CriarSut();

            await sut.Excluir(1);

            _planoRepo.Verify(r => r.Excluir(plano), Times.Once);
        }

        [Fact]
        public async Task ObterPlanoAtivo_DelegaParaRepositorio()
        {
            var plano = new PlanoEstudo { PlanoId = 1 };
            _planoRepo.Setup(r => r.ObterPlanoAtivo(1)).ReturnsAsync(plano);
            _planoRepo.Setup(r => r.ObterPlanoAtivoComTracking(1)).ReturnsAsync(plano);
            var sut = CriarSut();

            Assert.Same(plano, await sut.ObterPlanoAtivo(1));
            Assert.Same(plano, await sut.ObterPlanoAtivoComTracking(1));
        }

        [Fact]
        public async Task Compartilhar_PlanoDeOutroUsuario_LancaNaoEncontrado()
        {
            _planoRepo.Setup(r => r.ObterPlanoPorId(1))
                .ReturnsAsync(new PlanoEstudo { PlanoId = 1, UsuarioId = 2 });
            var sut = CriarSut();

            await Assert.ThrowsAsync<PlanoNaoEncontradoException>(() => sut.Compartilhar(1, 1));
        }

        [Fact]
        public async Task Compartilhar_PlanoJaCompartilhado_ReaproveitaGrupo()
        {
            var grupo = new GrupoEstudo { GrupoId = 3, Chave = "ABCD1234" };
            _planoRepo.Setup(r => r.ObterPlanoPorId(1))
                .ReturnsAsync(new PlanoEstudo { PlanoId = 1, UsuarioId = 1, GrupoId = 3 });
            _planoRepo.Setup(r => r.ObterGrupoPorId(3)).ReturnsAsync(grupo);
            var sut = CriarSut();

            Assert.Same(grupo, await sut.Compartilhar(1, 1));
            _planoRepo.Verify(r => r.CriarGrupo(It.IsAny<GrupoEstudo>()), Times.Never);
        }

        [Fact]
        public async Task Compartilhar_PrimeiraVez_CriaGrupoComChaveInedita()
        {
            var plano = new PlanoEstudo { PlanoId = 1, UsuarioId = 1 };
            _planoRepo.Setup(r => r.ObterPlanoPorId(1)).ReturnsAsync(plano);
            _planoRepo.SetupSequence(r => r.ChaveExiste(It.IsAny<string>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            _planoRepo.Setup(r => r.CriarGrupo(It.IsAny<GrupoEstudo>()))
                .Callback<GrupoEstudo>(g => g.GrupoId = 3)
                .Returns(Task.CompletedTask);
            var sut = CriarSut();

            var grupo = await sut.Compartilhar(1, 1);

            Assert.Equal(8, grupo.Chave.Length);
            Assert.Equal(1, grupo.CriadorId);
            Assert.Equal(3, plano.GrupoId);
            _planoRepo.Verify(r => r.ChaveExiste(It.IsAny<string>()), Times.Exactly(2));
            _planoRepo.Verify(r => r.Atualizar(It.Is<List<PlanoEstudo>>(l => l.Single() == plano)), Times.Once);
        }

        [Fact]
        public async Task Resgatar_ChaveInexistente_LancaChaveInvalida()
        {
            _planoRepo.Setup(r => r.ObterGrupoPorChave(It.IsAny<string>())).ReturnsAsync((GrupoEstudo)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<ChaveCompartilhamentoInvalidaException>(() => sut.Resgatar("abcd1234", 1));
        }

        [Fact]
        public async Task Resgatar_CriadorDoGrupo_LancaJaResgatado()
        {
            _planoRepo.Setup(r => r.ObterGrupoPorChave("ABCD1234"))
                .ReturnsAsync(new GrupoEstudo { GrupoId = 3, CriadorId = 1 });
            var sut = CriarSut();

            await Assert.ThrowsAsync<PlanoJaResgatadoException>(() => sut.Resgatar(" abcd1234 ", 1));
        }

        [Fact]
        public async Task Resgatar_UsuarioJaTemOPlano_LancaJaResgatado()
        {
            _planoRepo.Setup(r => r.ObterGrupoPorChave("ABCD1234"))
                .ReturnsAsync(new GrupoEstudo { GrupoId = 3, CriadorId = 2 });
            _planoRepo.Setup(r => r.ObterPlanoDoGrupoPorUsuario(3, 1))
                .ReturnsAsync(new PlanoEstudo { PlanoId = 9 });
            var sut = CriarSut();

            await Assert.ThrowsAsync<PlanoJaResgatadoException>(() => sut.Resgatar("abcd1234", 1));
        }

        [Fact]
        public async Task Resgatar_LimiteDePlanosAtingido_LancaLimitePlanos()
        {
            _planoRepo.Setup(r => r.ObterGrupoPorChave("ABCD1234"))
                .ReturnsAsync(new GrupoEstudo { GrupoId = 3, CriadorId = 2 });
            _planoRepo.Setup(r => r.ObterPlanoDoGrupoPorUsuario(3, 1)).ReturnsAsync((PlanoEstudo)null);
            _planoRepo.Setup(r => r.ContarPorUsuario(1)).ReturnsAsync(5);
            var sut = CriarSut();

            await Assert.ThrowsAsync<LimitePlanosAtingidoException>(() => sut.Resgatar("abcd1234", 1));
        }

        [Fact]
        public async Task Resgatar_PlanoDoCriadorAusente_LancaNaoEncontrado()
        {
            _planoRepo.Setup(r => r.ObterGrupoPorChave("ABCD1234"))
                .ReturnsAsync(new GrupoEstudo { GrupoId = 3, CriadorId = 2 });
            _planoRepo.Setup(r => r.ObterPlanoDoGrupoPorUsuario(3, It.IsAny<int>())).ReturnsAsync((PlanoEstudo)null);
            _planoRepo.Setup(r => r.ContarPorUsuario(1)).ReturnsAsync(0);
            var sut = CriarSut();

            await Assert.ThrowsAsync<PlanoNaoEncontradoException>(() => sut.Resgatar("abcd1234", 1));
        }

        [Fact]
        public async Task Resgatar_Valido_ClonaPlanoZerandoHorasConcluidas()
        {
            _planoRepo.Setup(r => r.ObterGrupoPorChave("ABCD1234"))
                .ReturnsAsync(new GrupoEstudo { GrupoId = 3, CriadorId = 2 });
            _planoRepo.Setup(r => r.ObterPlanoDoGrupoPorUsuario(3, 1)).ReturnsAsync((PlanoEstudo)null);
            _planoRepo.Setup(r => r.ContarPorUsuario(1)).ReturnsAsync(0);
            _planoRepo.Setup(r => r.ObterPlanoDoGrupoPorUsuario(3, 2)).ReturnsAsync(new PlanoEstudo
            {
                PlanoId = 9,
                Titulo = "Plano do amigo",
                Objetivo = "ENEM",
                HorasPorSemana = 12,
                PlanoMaterias = new List<PlanoMateria>
                {
                    new() { MateriaId = 4, HorasTotais = 10, HorasConcluidas = 8, Topicos = new List<string> { "Tópico" } }
                }
            });
            var sut = CriarSut();

            var clone = await sut.Resgatar("abcd1234", 1);

            Assert.Equal("Plano do amigo", clone.Titulo);
            Assert.Equal(1, clone.UsuarioId);
            Assert.Equal(3, clone.GrupoId);
            Assert.False(clone.Ativo);
            var materia = Assert.Single(clone.PlanoMaterias);
            Assert.Equal(4, materia.MateriaId);
            Assert.Equal(10, materia.HorasTotais);
            Assert.Equal(0, materia.HorasConcluidas);
            Assert.Equal(new[] { "Tópico" }, materia.Topicos);
            _planoRepo.Verify(r => r.Criar(clone), Times.Once);
        }

        [Fact]
        public async Task ObterGrupo_UsuarioForaDoGrupo_LancaNaoEncontrado()
        {
            _planoRepo.Setup(r => r.ListarPlanosDoGrupo(3)).ReturnsAsync(new List<PlanoEstudo>
            {
                new() { UsuarioId = 2, PlanoMaterias = new List<PlanoMateria>() }
            });
            var sut = CriarSut();

            await Assert.ThrowsAsync<PlanoNaoEncontradoException>(() => sut.ObterGrupo(3, 1));
        }

        [Fact]
        public async Task ObterGrupo_GrupoInexistente_LancaNaoEncontrado()
        {
            _planoRepo.Setup(r => r.ListarPlanosDoGrupo(3)).ReturnsAsync(new List<PlanoEstudo>
            {
                new() { UsuarioId = 1, PlanoMaterias = new List<PlanoMateria>() }
            });
            _planoRepo.Setup(r => r.ObterGrupoPorId(3)).ReturnsAsync((GrupoEstudo)null);
            var sut = CriarSut();

            await Assert.ThrowsAsync<PlanoNaoEncontradoException>(() => sut.ObterGrupo(3, 1));
        }

        [Fact]
        public async Task ObterGrupo_Criador_VeChaveEMembrosOrdenadosPorProgresso()
        {
            _planoRepo.Setup(r => r.ListarPlanosDoGrupo(3)).ReturnsAsync(new List<PlanoEstudo>
            {
                new()
                {
                    UsuarioId = 1,
                    Usuario = new Usuario { Id = 1, Nome = "João" },
                    PlanoMaterias = new List<PlanoMateria>
                    {
                        new() { HorasTotais = 10, HorasConcluidas = 2 }
                    }
                },
                new()
                {
                    UsuarioId = 2,
                    Usuario = new Usuario { Id = 2, Nome = "Maria" },
                    PlanoMaterias = new List<PlanoMateria>
                    {
                        new() { HorasTotais = 10, HorasConcluidas = 7 }
                    }
                },
                new()
                {
                    UsuarioId = 3,
                    Usuario = new Usuario { Id = 3, Nome = "Zeca" },
                    PlanoMaterias = new List<PlanoMateria>()
                }
            });
            _planoRepo.Setup(r => r.ObterGrupoPorId(3))
                .ReturnsAsync(new GrupoEstudo { GrupoId = 3, CriadorId = 1, Chave = "ABCD1234" });
            var sut = CriarSut();

            var progresso = await sut.ObterGrupo(3, 1);

            Assert.Equal("ABCD1234", progresso.Chave);
            Assert.Equal(new[] { "Maria", "João", "Zeca" }, progresso.Membros.Select(m => m.Nome));
            Assert.Equal(70, progresso.Membros[0].Percentual);
            Assert.Equal(0, progresso.Membros[2].Percentual);
            Assert.True(progresso.Membros[1].Eu);
        }

        [Fact]
        public async Task ObterGrupo_MembroConvidado_NaoRecebeAChave()
        {
            _planoRepo.Setup(r => r.ListarPlanosDoGrupo(3)).ReturnsAsync(new List<PlanoEstudo>
            {
                new() { UsuarioId = 1, PlanoMaterias = new List<PlanoMateria>() }
            });
            _planoRepo.Setup(r => r.ObterGrupoPorId(3))
                .ReturnsAsync(new GrupoEstudo { GrupoId = 3, CriadorId = 2, Chave = "ABCD1234" });
            var sut = CriarSut();

            Assert.Null((await sut.ObterGrupo(3, 1)).Chave);
        }
    }
}
