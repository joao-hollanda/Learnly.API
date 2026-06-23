using FluentValidation;
using Learnly.Api.Models.Redacoes.Request;

namespace Learnly.Api.Validators
{
    public class CorrigirRedacaoRequestValidator : AbstractValidator<CorrigirRedacaoRequest>
    {
        public CorrigirRedacaoRequestValidator()
        {
            RuleFor(x => x.Tema)
                .NotEmpty().WithMessage("Informe o tema da redação.")
                .MaximumLength(300).WithMessage("O tema deve ter no máximo 300 caracteres.");

            RuleFor(x => x.Texto)
                .NotEmpty().WithMessage("Informe o texto da redação.")
                .MinimumLength(120).WithMessage("A redação está muito curta para correção.")
                .MaximumLength(3500).WithMessage("A redação excede o tamanho máximo permitido.");
        }
    }
}
