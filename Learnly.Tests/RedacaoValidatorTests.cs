using FluentValidation.TestHelper;
using Learnly.Application.Validators;
using Learnly.Domain.Entities.Redacoes;
using Xunit;

namespace Learnly.Tests
{
    public class RedacaoValidatorTests
    {
        private readonly RedacaoValidator _validator = new();

        private static Redacao RedacaoValida() => new()
        {
            UsuarioId = 1,
            Tema = "Desafios da mobilidade urbana",
            Texto = new string('a', 200)
        };

        [Fact]
        public void RedacaoValida_NaoDeveTerErro()
        {
            _validator.TestValidate(RedacaoValida()).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void UsuarioIdZero_DeveTerErro()
        {
            var redacao = RedacaoValida();
            redacao.UsuarioId = 0;

            _validator.TestValidate(redacao).ShouldHaveValidationErrorFor(x => x.UsuarioId);
        }

        [Fact]
        public void TemaVazio_DeveTerErro()
        {
            var redacao = RedacaoValida();
            redacao.Tema = "";

            _validator.TestValidate(redacao).ShouldHaveValidationErrorFor(x => x.Tema);
        }

        [Fact]
        public void TemaAcimaDe300Caracteres_DeveTerErro()
        {
            var redacao = RedacaoValida();
            redacao.Tema = new string('a', 301);

            _validator.TestValidate(redacao).ShouldHaveValidationErrorFor(x => x.Tema);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(119)]
        [InlineData(3501)]
        public void TextoForaDosLimites_DeveTerErro(int tamanho)
        {
            var redacao = RedacaoValida();
            redacao.Texto = new string('a', tamanho);

            _validator.TestValidate(redacao).ShouldHaveValidationErrorFor(x => x.Texto);
        }

        [Fact]
        public void TextoNoLimiteMinimo_NaoDeveTerErro()
        {
            var redacao = RedacaoValida();
            redacao.Texto = new string('a', 120);

            _validator.TestValidate(redacao).ShouldNotHaveValidationErrorFor(x => x.Texto);
        }
    }
}
