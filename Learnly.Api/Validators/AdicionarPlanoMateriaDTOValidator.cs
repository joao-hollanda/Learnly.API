using FluentValidation;
using Learnly.Api.Models.Planos.Request;

namespace Learnly.Api.Validators
{
    public class AdicionarPlanoMateriaDTOValidator : AbstractValidator<AdicionarPlanoMateriaDTO>
    {
        public AdicionarPlanoMateriaDTOValidator()
        {
            RuleFor(x => x.MateriaId)
                .GreaterThan(0).WithMessage("A matéria é obrigatória.");

            RuleFor(x => x.HorasTotais)
                .GreaterThan(0).WithMessage("As horas totais devem ser maiores que zero.")
                .LessThanOrEqualTo(10000).WithMessage("As horas totais não podem exceder 10.000.");
        }
    }
}
