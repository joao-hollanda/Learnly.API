using FluentValidation.TestHelper;
using Learnly.Application.DTOs;
using Learnly.Application.Validators;
using Learnly.Domain.Entities;
using Xunit;

namespace Learnly.Tests
{
    public class EventoEstudoValidatorTests
    {
        private readonly EventoEstudoValidator _validator = new();

        private static EventoEstudo EventoValido() => new()
        {
            Titulo = "Revisão de matemática",
            Inicio = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc),
            Fim = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            UsuarioId = 1
        };

        [Fact]
        public void EventoValido_NaoDeveTerErro()
        {
            _validator.TestValidate(EventoValido()).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void TituloVazio_DeveTerErro()
        {
            var evento = EventoValido();
            evento.Titulo = "";

            _validator.TestValidate(evento).ShouldHaveValidationErrorFor(x => x.Titulo);
        }

        [Fact]
        public void TituloAcimaDe200Caracteres_DeveTerErro()
        {
            var evento = EventoValido();
            evento.Titulo = new string('a', 201);

            _validator.TestValidate(evento).ShouldHaveValidationErrorFor(x => x.Titulo);
        }

        [Fact]
        public void FimIgualAoInicio_DeveTerErro()
        {
            var evento = EventoValido();
            evento.Fim = evento.Inicio;

            _validator.TestValidate(evento).ShouldHaveValidationErrorFor(x => x.Fim);
        }

        [Fact]
        public void UsuarioIdZero_DeveTerErro()
        {
            var evento = EventoValido();
            evento.UsuarioId = 0;

            _validator.TestValidate(evento).ShouldHaveValidationErrorFor(x => x.UsuarioId);
        }
    }

    public class CriarEventoEstudoDtoValidatorTests
    {
        private readonly CriarEventoEstudoDtoValidator _validator = new();

        [Fact]
        public void DtoValido_NaoDeveTerErro()
        {
            var inicio = DateTime.UtcNow;

            _validator
                .TestValidate(new CriarEventoEstudoDto { Titulo = "Revisão", Inicio = inicio, Fim = inicio.AddHours(1) })
                .ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void TituloVazio_DeveTerErro()
        {
            var inicio = DateTime.UtcNow;

            _validator
                .TestValidate(new CriarEventoEstudoDto { Titulo = "", Inicio = inicio, Fim = inicio.AddHours(1) })
                .ShouldHaveValidationErrorFor(x => x.Titulo);
        }

        [Fact]
        public void FimAntesDoInicio_DeveTerErro()
        {
            var inicio = DateTime.UtcNow;

            _validator
                .TestValidate(new CriarEventoEstudoDto { Titulo = "Revisão", Inicio = inicio, Fim = inicio.AddHours(-1) })
                .ShouldHaveValidationErrorFor(x => x.Fim);
        }
    }

    public class CriarEventosEstudoLoteDtoValidatorTests
    {
        private readonly CriarEventosEstudoLoteDtoValidator _validator = new();

        private static CriarEventoEstudoDto Evento(int indice)
        {
            var inicio = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc).AddDays(indice);
            return new CriarEventoEstudoDto { Titulo = $"Estudo {indice}", Inicio = inicio, Fim = inicio.AddHours(1) };
        }

        [Fact]
        public void ListaVazia_DeveTerErro()
        {
            _validator
                .TestValidate(new CriarEventosEstudoLoteDto())
                .ShouldHaveValidationErrorFor(x => x.Eventos);
        }

        [Fact]
        public void AcimaDeCemEventos_DeveTerErro()
        {
            var dto = new CriarEventosEstudoLoteDto
            {
                Eventos = Enumerable.Range(0, 101).Select(Evento).ToList()
            };

            _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Eventos);
        }

        [Fact]
        public void CemEventosValidos_NaoDeveTerErro()
        {
            var dto = new CriarEventosEstudoLoteDto
            {
                Eventos = Enumerable.Range(0, 100).Select(Evento).ToList()
            };

            _validator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EventoInternoInvalido_DeveTerErro()
        {
            var invalido = Evento(0);
            invalido.Fim = invalido.Inicio.AddHours(-1);
            var dto = new CriarEventosEstudoLoteDto { Eventos = new List<CriarEventoEstudoDto> { invalido } };

            _validator.TestValidate(dto).ShouldHaveValidationErrorFor("Eventos[0].Fim");
        }
    }
}
