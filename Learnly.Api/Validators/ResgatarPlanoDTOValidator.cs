using FluentValidation;
using Learnly.Api.Models.Planos.Request;

namespace Learnly.Api.Validators
{
    public class ResgatarPlanoDTOValidator : AbstractValidator<ResgatarPlanoDTO>
    {
        public ResgatarPlanoDTOValidator()
        {
            RuleFor(x => x.Chave)
                .NotEmpty().WithMessage("A chave de compartilhamento é obrigatória.")
                .MaximumLength(12).WithMessage("Chave de compartilhamento inválida.");
        }
    }
}
