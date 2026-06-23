using FluentValidation;
using Learnly.Domain.Entities.Redacoes;

namespace Learnly.Application.Validators
{
    public class RedacaoValidator : AbstractValidator<Redacao>
    {
        public RedacaoValidator()
        {
            RuleFor(x => x.UsuarioId)
                .GreaterThan(0).WithMessage("O usuário é obrigatório.");

            RuleFor(x => x.Tema)
                .NotEmpty().WithMessage("O tema da redação é obrigatório.")
                .MaximumLength(300).WithMessage("O tema deve ter no máximo 300 caracteres.");

            RuleFor(x => x.Texto)
                .NotEmpty().WithMessage("O texto da redação é obrigatório.")
                .MinimumLength(120).WithMessage("A redação está muito curta para correção.")
                .MaximumLength(3500).WithMessage("A redação excede o tamanho máximo permitido.");
        }
    }
}
