using FluentValidation;
using Learnly.Domain.Entities;

namespace Learnly.Application.Validators
{
    public class EventoEstudoValidator : AbstractValidator<EventoEstudo>
    {
        public EventoEstudoValidator()
        {
            RuleFor(x => x.Titulo)
                .NotEmpty().WithMessage("O título do evento é obrigatório.")
                .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.");

            RuleFor(x => x.Inicio)
                .NotEmpty().WithMessage("A data/hora de início é obrigatória.");

            RuleFor(x => x.Fim)
                .NotEmpty().WithMessage("A data/hora de fim é obrigatória.")
                .GreaterThan(x => x.Inicio).WithMessage("O fim do evento deve ser posterior ao início.");

            RuleFor(x => x.UsuarioId)
                .GreaterThan(0).WithMessage("O usuário é obrigatório.");
        }
    }
}
