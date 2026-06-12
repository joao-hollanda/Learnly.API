using FluentValidation;
using FluentValidation.Results;
using Learnly.Application.Applications;
using Learnly.Application.DTOs;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Learnly.Domain.Exceptions.Planos;
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
    }
}
