using FluentValidation.TestHelper;
using Learnly.Application.Validators;
using Learnly.Domain.Entities;
using Xunit;

namespace Learnly.Tests
{
    public class UsuarioValidatorTests
    {
        private readonly UsuarioValidator _validator = new();

        [Fact]
        public void NomeVazio_DeveTerErro()
        {
            var resultado = _validator.TestValidate(
                new Usuario { Nome = "", Email = "joao@learnly.com.br" });

            resultado.ShouldHaveValidationErrorFor(x => x.Nome);
        }

        [Fact]
        public void EmailInvalido_DeveTerErro()
        {
            var resultado = _validator.TestValidate(
                new Usuario { Nome = "João", Email = "email-invalido" });

            resultado.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void UsuarioValido_NaoDeveTerErro()
        {
            var resultado = _validator.TestValidate(
                new Usuario { Nome = "João", Email = "joao@learnly.com.br" });

            resultado.ShouldNotHaveAnyValidationErrors();
        }
    }
}
