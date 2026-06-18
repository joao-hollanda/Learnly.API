using FluentValidation;
using Learnly.Domain.Entities.Social;

namespace Learnly.Application.Validators
{
    public class GrupoValidator : AbstractValidator<Grupo>
    {
        public GrupoValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome do grupo é obrigatório.")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Descricao)
                .MaximumLength(300).WithMessage("A descrição deve ter no máximo 300 caracteres.");

            RuleFor(x => x.CriadorId)
                .GreaterThan(0).WithMessage("O criador é obrigatório.");
        }
    }
}
