using Learnly.Domain.Exceptions.Comuns;

namespace Learnly.Domain.Exceptions.Usuarios
{
    public class SenhaInvalidaException : DomainException
    {
        public SenhaInvalidaException()
            : base("Senha inválida. A senha deve ter no mínimo 8 caracteres, incluindo letra maiúscula, minúscula, número e caractere especial.") { }

        public SenhaInvalidaException(string mensagem)
            : base(mensagem) { }
    }
}
