using FluentValidation;
using Learnly.Domain.Entities.Social;

namespace Learnly.Application.Validators
{
    public class MensagemValidator : AbstractValidator<Mensagem>
    {
        public MensagemValidator()
        {
            RuleFor(x => x.ConversaId)
                .GreaterThan(0).WithMessage("A conversa é obrigatória.");

            RuleFor(x => x.RemetenteId)
                .GreaterThan(0).WithMessage("O remetente é obrigatório.");

            RuleFor(x => x.Texto)
                .MaximumLength(4000).WithMessage("A mensagem deve ter no máximo 4000 caracteres.");
        }
    }
}
