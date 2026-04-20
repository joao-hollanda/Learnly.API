using FluentValidation;

namespace Learnly.Application.Validators
{
    public class SimuladoValidator : AbstractValidator<Simulado>
    {
        public SimuladoValidator()
        {
            RuleFor(x => x.UsuarioId)
                .GreaterThan(0).WithMessage("O usuário é obrigatório.");

            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("A data do simulado é obrigatória.");
        }
    }
}
