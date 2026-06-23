using FluentValidation;
using Learnly.Api.Models.Usuarios.Request;

namespace Learnly.Api.Validators
{
    public class ConfirmarEmailRequestValidator : AbstractValidator<ConfirmarEmailRequest>
    {
        public ConfirmarEmailRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token de confirmação ausente.");
        }
    }
}
