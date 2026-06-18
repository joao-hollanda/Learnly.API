using FluentValidation;
using Learnly.Api.Models.Social.Request;

namespace Learnly.Api.Validators
{
    public class CriarGrupoRequestValidator : AbstractValidator<CriarGrupoRequest>
    {
        public CriarGrupoRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome do grupo é obrigatório.")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Descricao)
                .MaximumLength(300).WithMessage("A descrição deve ter no máximo 300 caracteres.");
        }
    }
}
