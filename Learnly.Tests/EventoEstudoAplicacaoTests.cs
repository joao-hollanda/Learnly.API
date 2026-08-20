using FluentValidation;
using Learnly.Application.Applications;
using Learnly.Application.DTOs;
using Learnly.Application.Validators;
using Learnly.Domain.Entities;
using Learnly.Repository.Interfaces;
using Moq;
using Xunit;

namespace Learnly.Tests
{
    public class EventoEstudoAplicacaoTests
    {
        private readonly Mock<IEventoEstudoRepositorio> _repositorio = new();

        private EventoEstudoAplicacao CriarSut() => new(
            _repositorio.Object,
            new EventoEstudoValidator(),
            new CriarEventoEstudoDtoValidator());

        [Fact]
        public async Task Listar_DelegaParaRepositorio()
        {
            var eventos = new List<EventoEstudo> { new() { EventoEstudoId = 1 } };
            _repositorio.Setup(r => r.ObterPorUsuario(1)).ReturnsAsync(eventos);
            var sut = CriarSut();

            Assert.Same(eventos, await sut.Listar(1));
        }

        [Fact]
        public async Task Criar_FimAntesDoInicio_LancaValidationException()
        {
            var inicio = DateTime.UtcNow;
            var sut = CriarSut();

            await Assert.ThrowsAsync<ValidationException>(
                () => sut.Criar("Revisão", inicio, inicio.AddHours(-1), 1));
            _repositorio.Verify(r => r.Adicionar(It.IsAny<EventoEstudo>()), Times.Never);
        }

        [Fact]
        public async Task Criar_TituloVazio_LancaValidationException()
        {
            var inicio = DateTime.UtcNow;
            var sut = CriarSut();

            await Assert.ThrowsAsync<ValidationException>(
                () => sut.Criar("", inicio, inicio.AddHours(2), 1));
        }

        [Fact]
        public async Task Criar_Valido_AdicionaEventoDoUsuario()
        {
            var inicio = DateTime.UtcNow;
            var sut = CriarSut();

            await sut.Criar("Revisão", inicio, inicio.AddHours(2), 1);

            _repositorio.Verify(r => r.Adicionar(It.Is<EventoEstudo>(e =>
                e.Titulo == "Revisão" &&
                e.Inicio == inicio &&
                e.UsuarioId == 1)), Times.Once);
        }

        [Fact]
        public async Task Remover_DelegaParaRepositorio()
        {
            var sut = CriarSut();

            await sut.Remover(9);

            _repositorio.Verify(r => r.Remover(9), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CriarEmLote_ListaVazia_LancaArgumentException(bool nula)
        {
            var sut = CriarSut();

            await Assert.ThrowsAsync<ArgumentException>(
                () => sut.CriarEmLote(1, nula ? null : new List<CriarEventoEstudoDto>()));
        }

        [Fact]
        public async Task CriarEmLote_EventoInvalido_LancaValidationException()
        {
            var inicio = DateTime.UtcNow;
            var sut = CriarSut();

            await Assert.ThrowsAsync<ValidationException>(() => sut.CriarEmLote(1, new List<CriarEventoEstudoDto>
            {
                new() { Titulo = "Válido", Inicio = inicio, Fim = inicio.AddHours(1) },
                new() { Titulo = "Inválido", Inicio = inicio, Fim = inicio.AddHours(-1) }
            }));
            _repositorio.Verify(r => r.AdicionarEmLote(It.IsAny<List<EventoEstudo>>()), Times.Never);
        }

        [Fact]
        public async Task CriarEmLote_Valido_AtribuiUsuarioATodosOsEventos()
        {
            var inicio = DateTime.UtcNow;
            var sut = CriarSut();

            await sut.CriarEmLote(7, new List<CriarEventoEstudoDto>
            {
                new() { Titulo = "Matemática", Inicio = inicio, Fim = inicio.AddHours(1) },
                new() { Titulo = "Física", Inicio = inicio.AddDays(1), Fim = inicio.AddDays(1).AddHours(1) }
            });

            _repositorio.Verify(r => r.AdicionarEmLote(It.Is<List<EventoEstudo>>(eventos =>
                eventos.Count == 2 &&
                eventos.All(e => e.UsuarioId == 7) &&
                eventos[0].Titulo == "Matemática" &&
                eventos[1].Titulo == "Física")), Times.Once);
        }
    }
}
