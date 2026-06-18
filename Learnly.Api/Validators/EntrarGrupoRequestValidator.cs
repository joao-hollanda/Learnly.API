using FluentValidation;
using Learnly.Api.Models.Social.Request;

namespace Learnly.Api.Validators
{
    public class EntrarGrupoRequestValidator : AbstractValidator<EntrarGrupoRequest>
    {
        public EntrarGrupoRequestValidator()
        {
            RuleFor(x => x.Chave)
                .NotEmpty().WithMessage("O código do grupo é obrigatório.")
                .MaximumLength(12).WithMessage("Código de grupo inválido.");
        }
    }
}
