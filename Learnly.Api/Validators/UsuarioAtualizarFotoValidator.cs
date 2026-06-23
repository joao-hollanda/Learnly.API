using FluentValidation;
using Learnly.Api.Models.Usuarios.Request;

namespace Learnly.Api.Validators
{
    public class UsuarioAtualizarFotoValidator : AbstractValidator<UsuarioAtualizarFoto>
    {
        public UsuarioAtualizarFotoValidator()
        {
            RuleFor(x => x.Foto)
                .MaximumLength(1_500_000).WithMessage("A imagem é muito grande. Escolha uma foto menor.");
        }
    }
}
