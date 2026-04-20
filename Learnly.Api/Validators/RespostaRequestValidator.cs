using FluentValidation;
using Learnly.Api.Models.Simulados.Request;

namespace Learnly.Api.Validators
{
    public class RespostaRequestValidator : AbstractValidator<RespostaRequest>
    {
        public RespostaRequestValidator()
        {
            RuleFor(x => x.QuestaoId)
                .GreaterThan(0).WithMessage("A questão é obrigatória.");

            RuleFor(x => x.AlternativaId)
                .GreaterThan(0).WithMessage("A alternativa é obrigatória.");
        }
    }
}
