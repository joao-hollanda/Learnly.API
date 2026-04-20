using FluentValidation;
using Learnly.Api.Models.Planos.Request;

namespace Learnly.Api.Validators
{
    public class CriarPlanoDTOValidator : AbstractValidator<CriarPlanoDTO>
    {
        public CriarPlanoDTOValidator()
        {
            RuleFor(x => x.Titulo)
                .NotEmpty().WithMessage("O título é obrigatório.")
                .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.");

            RuleFor(x => x.Objetivo)
                .NotEmpty().WithMessage("O objetivo é obrigatório.")
                .MaximumLength(500).WithMessage("O objetivo deve ter no máximo 500 caracteres.");

            RuleFor(x => x.DataInicio)
                .NotEmpty().WithMessage("A data de início é obrigatória.");

            RuleFor(x => x.DataFim)
                .NotEmpty().WithMessage("A data de fim é obrigatória.")
                .GreaterThan(x => x.DataInicio).WithMessage("A data de fim deve ser posterior à data de início.");

            RuleFor(x => x.HorasPorSemana)
                .GreaterThanOrEqualTo(0).WithMessage("As horas por semana não podem ser negativas.")
                .LessThanOrEqualTo(168).WithMessage("As horas por semana não podem exceder 168.");
        }
    }
}
