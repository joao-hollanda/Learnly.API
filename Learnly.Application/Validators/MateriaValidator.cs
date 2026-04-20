using FluentValidation;
using Learnly.Domain.Entities.Planos;

namespace Learnly.Application.Validators
{
    public class MateriaValidator : AbstractValidator<Materia>
    {
        public MateriaValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome da matéria é obrigatório.")
                .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Cor)
                .NotEmpty().WithMessage("A cor é obrigatória.")
                .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Informe uma cor em formato hexadecimal válido (ex: #FF5733).");
        }
    }
}
