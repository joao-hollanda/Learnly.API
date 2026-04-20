using FluentValidation;
using Learnly.Api.Models.Simulados.Request;

namespace Learnly.Api.Validators
{
    public class SimuladoRequestValidator : AbstractValidator<SimuladoRequest>
    {
        public SimuladoRequestValidator()
        {
            RuleFor(x => x.Disciplinas)
                .NotEmpty().WithMessage("Informe ao menos uma disciplina.")
                .Must(d => d.Count <= 10).WithMessage("Não é possível selecionar mais de 10 disciplinas.");

            RuleForEach(x => x.Disciplinas)
                .NotEmpty().WithMessage("O nome da disciplina não pode ser vazio.")
                .MaximumLength(100).WithMessage("O nome da disciplina deve ter no máximo 100 caracteres.");

            RuleFor(x => x.QuantidadeQuestoes)
                .GreaterThan(0).WithMessage("A quantidade de questões deve ser maior que zero.")
                .LessThanOrEqualTo(100).WithMessage("A quantidade de questões não pode exceder 100.");
        }
    }
}
