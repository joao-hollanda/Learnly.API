using FluentValidation.TestHelper;
using Learnly.Application.DTOs;
using Learnly.Application.Validators;
using Learnly.Domain.Entities;
using Xunit;

namespace Learnly.Tests
{
    public class PlanoEstudoValidatorTests
    {
        private readonly PlanoEstudoValidator _validator = new();

        private static PlanoEstudo PlanoValido() => new()
        {
            Titulo = "Plano ENEM",
            Objetivo = "Passar em medicina",
            DataInicio = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DataFim = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            HorasPorSemana = 20,
            UsuarioId = 1
        };

        [Fact]
        public void PlanoValido_NaoDeveTerErro()
        {
            _validator.TestValidate(PlanoValido()).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void TituloVazio_DeveTerErro()
        {
            var plano = PlanoValido();
            plano.Titulo = "";

            _validator.TestValidate(plano).ShouldHaveValidationErrorFor(x => x.Titulo);
        }

        [Fact]
        public void TituloAcimaDe200Caracteres_DeveTerErro()
        {
            var plano = PlanoValido();
            plano.Titulo = new string('a', 201);

            _validator.TestValidate(plano).ShouldHaveValidationErrorFor(x => x.Titulo);
        }

        [Fact]
        public void ObjetivoVazio_DeveTerErro()
        {
            var plano = PlanoValido();
            plano.Objetivo = "";

            _validator.TestValidate(plano).ShouldHaveValidationErrorFor(x => x.Objetivo);
        }

        [Fact]
        public void ObjetivoAcimaDe500Caracteres_DeveTerErro()
        {
            var plano = PlanoValido();
            plano.Objetivo = new string('a', 501);

            _validator.TestValidate(plano).ShouldHaveValidationErrorFor(x => x.Objetivo);
        }

        [Fact]
        public void DataFimAntesDaDataInicio_DeveTerErro()
        {
            var plano = PlanoValido();
            plano.DataFim = plano.DataInicio.AddDays(-1);

            _validator.TestValidate(plano).ShouldHaveValidationErrorFor(x => x.DataFim);
        }

        [Fact]
        public void DuracaoMenorQueDuasSemanas_DeveTerErro()
        {
            var plano = PlanoValido();
            plano.DataFim = plano.DataInicio.AddDays(13);

            _validator.TestValidate(plano).ShouldHaveValidationErrorFor(x => x.DataFim);
        }

        [Fact]
        public void DuracaoDeExatamenteDuasSemanas_NaoDeveTerErro()
        {
            var plano = PlanoValido();
            plano.DataFim = plano.DataInicio.AddDays(14);

            _validator.TestValidate(plano).ShouldNotHaveValidationErrorFor(x => x.DataFim);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(169)]
        public void HorasPorSemanaForaDoIntervalo_DeveTerErro(int horas)
        {
            var plano = PlanoValido();
            plano.HorasPorSemana = horas;

            _validator.TestValidate(plano).ShouldHaveValidationErrorFor(x => x.HorasPorSemana);
        }

        [Fact]
        public void UsuarioIdZero_DeveTerErro()
        {
            var plano = PlanoValido();
            plano.UsuarioId = 0;

            _validator.TestValidate(plano).ShouldHaveValidationErrorFor(x => x.UsuarioId);
        }
    }

    public class CriarPlanoIADTOValidatorTests
    {
        private readonly CriarPlanoIADTOValidator _validator = new();

        private static CriarPlanoIADTO DtoValido() => new()
        {
            Titulo = "Plano ENEM",
            Objetivo = "Passar em medicina",
            DataInicio = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DataFim = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            HorasPorSemana = 20,
            UsuarioId = 1
        };

        [Fact]
        public void DtoValido_NaoDeveTerErro()
        {
            _validator.TestValidate(DtoValido()).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void TituloVazio_DeveTerErro()
        {
            var dto = DtoValido();
            dto.Titulo = "";

            _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Titulo);
        }

        [Fact]
        public void ObjetivoAcimaDe500Caracteres_DeveTerErro()
        {
            var dto = DtoValido();
            dto.Objetivo = new string('a', 501);

            _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Objetivo);
        }

        [Fact]
        public void DataFimAntesDaDataInicio_DeveTerErro()
        {
            var dto = DtoValido();
            dto.DataFim = dto.DataInicio.AddDays(-1);

            _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DataFim);
        }

        [Fact]
        public void DuracaoCurta_NaoDeveTerErro()
        {
            var dto = DtoValido();
            dto.DataFim = dto.DataInicio.AddDays(1);

            _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.DataFim);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(169)]
        public void HorasPorSemanaForaDoIntervalo_DeveTerErro(int horas)
        {
            var dto = DtoValido();
            dto.HorasPorSemana = horas;

            _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.HorasPorSemana);
        }

        [Fact]
        public void UsuarioIdZero_DeveTerErro()
        {
            var dto = DtoValido();
            dto.UsuarioId = 0;

            _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.UsuarioId);
        }
    }
}
