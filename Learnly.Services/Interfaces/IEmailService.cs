namespace Learnly.Application.Interfaces
{
    public interface IEmailService
    {
        Task EnviarConfirmacaoAsync(string para, string nome, string link);
        Task EnviarRecuperacaoSenhaAsync(string para, string nome, string link);
    }
}
