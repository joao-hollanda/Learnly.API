using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Usuarios
{
    public class UsuarioInativoException : DomainException
    {
        public UsuarioInativoException()
            : base("A conta do usuário está inativa.") { }
    }
}
