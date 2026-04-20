using FluentValidation;
using Learnly.Api.Models.Usuarios.Response;

namespace Learnly.Api.Validators
{
    public class UsuarioCriarValidator : AbstractValidator<UsuarioCriar>
    {
        public UsuarioCriarValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("Informe um e-mail válido.")
                .MaximumLength(150).WithMessage("O e-mail deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");

            RuleFor(x => x.Cidade)
                .MaximumLength(100).WithMessage("A cidade deve ter no máximo 100 caracteres.");
        }
    }
}
