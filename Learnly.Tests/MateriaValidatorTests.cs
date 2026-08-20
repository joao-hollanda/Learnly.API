using FluentValidation.TestHelper;
using Learnly.Application.Validators;
using Learnly.Domain.Entities.Planos;
using Xunit;

namespace Learnly.Tests
{
    public class MateriaValidatorTests
    {
        private readonly MateriaValidator _validator = new();

        [Theory]
        [InlineData("#FF5733")]
        [InlineData("#f57")]
        public void MateriaValida_NaoDeveTerErro(string cor)
        {
            _validator
                .TestValidate(new Materia { Nome = "Matemática", Cor = cor })
                .ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void NomeVazio_DeveTerErro()
        {
            _validator
                .TestValidate(new Materia { Nome = "", Cor = "#FF5733" })
                .ShouldHaveValidationErrorFor(x => x.Nome);
        }

        [Fact]
        public void NomeAcimaDe150Caracteres_DeveTerErro()
        {
            _validator
                .TestValidate(new Materia { Nome = new string('a', 151), Cor = "#FF5733" })
                .ShouldHaveValidationErrorFor(x => x.Nome);
        }

        [Theory]
        [InlineData("")]
        [InlineData("FF5733")]
        [InlineData("#GG5733")]
        [InlineData("#FF57")]
        public void CorInvalida_DeveTerErro(string cor)
        {
            _validator
                .TestValidate(new Materia { Nome = "Matemática", Cor = cor })
                .ShouldHaveValidationErrorFor(x => x.Cor);
        }
    }
}
