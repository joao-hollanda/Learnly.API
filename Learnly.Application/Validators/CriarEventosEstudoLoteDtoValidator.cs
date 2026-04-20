using FluentValidation;
using Learnly.Application.DTOs;

namespace Learnly.Application.Validators
{
    public class CriarEventosEstudoLoteDtoValidator : AbstractValidator<CriarEventosEstudoLoteDto>
    {
        public CriarEventosEstudoLoteDtoValidator()
        {
            RuleFor(x => x.UsuarioId)
                .GreaterThan(0).WithMessage("O usuário é obrigatório.");

            RuleFor(x => x.Eventos)
                .NotEmpty().WithMessage("Informe ao menos um evento.")
                .Must(e => e.Count <= 100).WithMessage("Não é possível criar mais de 100 eventos por vez.");

            RuleForEach(x => x.Eventos).SetValidator(new CriarEventoEstudoDtoValidator());
        }
    }
}
