using FluentValidation;
using Learnly.Api.Models.Usuarios.Request;

namespace Learnly.Api.Validators
{
    public class RedefinirSenhaRequestValidator : AbstractValidator<RedefinirSenhaRequest>
    {
        public RedefinirSenhaRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token de redefinição ausente.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A nova senha é obrigatória.")
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");
        }
    }
}
