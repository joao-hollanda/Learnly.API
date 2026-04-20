using FluentValidation;
using Learnly.Api.Models.Usuarios.Request;

namespace Learnly.Api.Validators
{
    public class UsuarioAtualizarSenhaValidator : AbstractValidator<UsuarioAtualizarSenha>
    {
        public UsuarioAtualizarSenhaValidator()
        {
            RuleFor(x => x.SenhaAntiga)
                .NotEmpty().WithMessage("A senha antiga é obrigatória.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A nova senha é obrigatória.")
                .MinimumLength(6).WithMessage("A nova senha deve ter no mínimo 6 caracteres.")
                .NotEqual(x => x.SenhaAntiga).WithMessage("A nova senha deve ser diferente da senha atual.");
        }
    }
}
