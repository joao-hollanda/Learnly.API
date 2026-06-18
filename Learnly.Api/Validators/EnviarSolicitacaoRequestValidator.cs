using FluentValidation;
using Learnly.Api.Models.Social.Request;

namespace Learnly.Api.Validators
{
    public class EnviarSolicitacaoRequestValidator : AbstractValidator<EnviarSolicitacaoRequest>
    {
        public EnviarSolicitacaoRequestValidator()
        {
            RuleFor(x => x.EmailOuNome)
                .NotEmpty().WithMessage("Informe o e-mail ou nome do usuário.")
                .MaximumLength(150).WithMessage("Valor muito longo.");
        }
    }
}
