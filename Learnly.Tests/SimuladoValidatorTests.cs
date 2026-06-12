using System;
using FluentValidation.TestHelper;
using Learnly.Application.Validators;
using Xunit;

namespace Learnly.Tests
{
    public class SimuladoValidatorTests
    {
        private readonly SimuladoValidator _validator = new();

        [Fact]
        public void UsuarioIdZero_DeveTerErro()
        {
            var resultado = _validator.TestValidate(
                new Simulado { UsuarioId = 0, Data = DateTime.UtcNow });

            resultado.ShouldHaveValidationErrorFor(x => x.UsuarioId);
        }

        [Fact]
        public void DataVazia_DeveTerErro()
        {
            var resultado = _validator.TestValidate(
                new Simulado { UsuarioId = 1, Data = default });

            resultado.ShouldHaveValidationErrorFor(x => x.Data);
        }

        [Fact]
        public void SimuladoValido_NaoDeveTerErro()
        {
            var resultado = _validator.TestValidate(
                new Simulado { UsuarioId = 1, Data = DateTime.UtcNow });

            resultado.ShouldNotHaveAnyValidationErrors();
        }
    }
}
