using FluentValidation.TestHelper;
using Learnly.Application.Validators;
using Learnly.Domain.Entities.Social;
using Xunit;

namespace Learnly.Tests
{
    public class GrupoValidatorTests
    {
        private readonly GrupoValidator _validator = new();

        private static Grupo GrupoValido() => new()
        {
            Nome = "Estudo ENEM",
            Descricao = "Turma da manhã",
            CriadorId = 1
        };

        [Fact]
        public void GrupoValido_NaoDeveTerErro()
        {
            _validator.TestValidate(GrupoValido()).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void NomeVazio_DeveTerErro()
        {
            var grupo = GrupoValido();
            grupo.Nome = "";

            _validator.TestValidate(grupo).ShouldHaveValidationErrorFor(x => x.Nome);
        }

        [Fact]
        public void NomeAcimaDe100Caracteres_DeveTerErro()
        {
            var grupo = GrupoValido();
            grupo.Nome = new string('a', 101);

            _validator.TestValidate(grupo).ShouldHaveValidationErrorFor(x => x.Nome);
        }

        [Fact]
        public void DescricaoAcimaDe300Caracteres_DeveTerErro()
        {
            var grupo = GrupoValido();
            grupo.Descricao = new string('a', 301);

            _validator.TestValidate(grupo).ShouldHaveValidationErrorFor(x => x.Descricao);
        }

        [Fact]
        public void DescricaoNula_NaoDeveTerErro()
        {
            var grupo = GrupoValido();
            grupo.Descricao = null;

            _validator.TestValidate(grupo).ShouldNotHaveValidationErrorFor(x => x.Descricao);
        }

        [Fact]
        public void CriadorIdZero_DeveTerErro()
        {
            var grupo = GrupoValido();
            grupo.CriadorId = 0;

            _validator.TestValidate(grupo).ShouldHaveValidationErrorFor(x => x.CriadorId);
        }
    }

    public class MensagemValidatorTests
    {
        private readonly MensagemValidator _validator = new();

        private static Mensagem MensagemValida() => new()
        {
            ConversaId = 1,
            RemetenteId = 2,
            Texto = "olá"
        };

        [Fact]
        public void MensagemValida_NaoDeveTerErro()
        {
            _validator.TestValidate(MensagemValida()).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void ConversaIdZero_DeveTerErro()
        {
            var mensagem = MensagemValida();
            mensagem.ConversaId = 0;

            _validator.TestValidate(mensagem).ShouldHaveValidationErrorFor(x => x.ConversaId);
        }

        [Fact]
        public void RemetenteIdZero_DeveTerErro()
        {
            var mensagem = MensagemValida();
            mensagem.RemetenteId = 0;

            _validator.TestValidate(mensagem).ShouldHaveValidationErrorFor(x => x.RemetenteId);
        }

        [Fact]
        public void TextoAcimaDe4000Caracteres_DeveTerErro()
        {
            var mensagem = MensagemValida();
            mensagem.Texto = new string('a', 4001);

            _validator.TestValidate(mensagem).ShouldHaveValidationErrorFor(x => x.Texto);
        }

        [Fact]
        public void TextoNulo_NaoDeveTerErro()
        {
            var mensagem = MensagemValida();
            mensagem.Texto = null;

            _validator.TestValidate(mensagem).ShouldNotHaveValidationErrorFor(x => x.Texto);
        }
    }
}
