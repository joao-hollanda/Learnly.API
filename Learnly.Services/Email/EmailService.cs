using System.Net.Http.Headers;
using System.Text;
using Learnly.Application.Interfaces;
using Newtonsoft.Json;

namespace Learnly.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient _http;
        private readonly EmailOptions _options;

        public EmailService(HttpClient http, EmailOptions options)
        {
            _http = http;
            _options = options;
        }

        public Task EnviarConfirmacaoAsync(string para, string nome, string link)
        {
            var corpo = $@"
                <p style=""margin:0 0 16px"">Olá, {nome}! Falta um passo para começar a estudar com a Learnly.</p>
                <p style=""margin:0 0 24px"">Confirme seu e-mail para ativar sua conta. O link expira em 24 horas.</p>";

            return EnviarAsync(para, "Confirme seu e-mail · Learnly", corpo, "Confirmar e-mail", link);
        }

        public Task EnviarRecuperacaoSenhaAsync(string para, string nome, string link)
        {
            var corpo = $@"
                <p style=""margin:0 0 16px"">Olá, {nome}! Recebemos um pedido para redefinir a senha da sua conta Learnly.</p>
                <p style=""margin:0 0 24px"">Clique no botão abaixo para criar uma nova senha. O link expira em 30 minutos. Se não foi você, ignore este e-mail.</p>";

            return EnviarAsync(para, "Redefinição de senha · Learnly", corpo, "Redefinir senha", link);
        }

        public Task EnviarLembreteSequenciaAsync(string para, string nome, int dias, string linkPlano, string linkDescadastro)
        {
            var corpo = $@"
                <p style=""margin:0 0 16px"">Olá, {nome}! Sua sequência de <strong>{dias} dias</strong> de estudo está em risco — você ainda não registrou horas hoje.</p>
                <p style=""margin:0 0 24px"">Mantenha o ritmo: lance seu estudo de hoje antes da meia-noite para não perder a sequência.</p>";

            var rodape = $@"<a href=""{linkDescadastro}"" style=""color:#94a3b8"">Não quero mais receber estes lembretes</a>";

            return EnviarAsync(para, $"Sua sequência de {dias} dias termina hoje · Learnly", corpo, "Lançar horas de hoje", linkPlano, rodape);
        }

        private async Task EnviarAsync(string para, string assunto, string corpoHtml, string textoBotao, string link, string rodapeExtra = null)
        {
            var html = MontarHtml(corpoHtml, textoBotao, link, rodapeExtra);

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                Console.WriteLine($"[EmailService] Resend sem ApiKey — e-mail '{assunto}' para {para} não enviado. Link: {link}");
                return;
            }

            var json = JsonConvert.SerializeObject(new
            {
                from = _options.From,
                to = new[] { para },
                subject = assunto,
                html
            });

            var requisicao = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var resposta = await _http.SendAsync(requisicao);

            if (!resposta.IsSuccessStatusCode)
            {
                var detalhe = await resposta.Content.ReadAsStringAsync();
                Console.WriteLine($"[EmailService] Resend retornou {(int)resposta.StatusCode} ao enviar '{assunto}' para {para}: {detalhe}");
                throw new Exception($"Resend retornou {(int)resposta.StatusCode}: {detalhe}");
            }
        }

        private static string MontarHtml(string corpo, string textoBotao, string link, string rodapeExtra = null) => $@"
<!DOCTYPE html>
<html lang=""pt-BR"">
<body style=""margin:0;background:#f1f5f9;font-family:Arial,Helvetica,sans-serif;color:#1e293b"">
  <div style=""max-width:520px;margin:0 auto;padding:32px 16px"">
    <div style=""background:#0f1f5c;border-radius:14px 14px 0 0;padding:28px 32px"">
      <span style=""color:#fff;font-size:22px;font-weight:800;letter-spacing:-0.5px"">Learnly</span>
    </div>
    <div style=""background:#fff;border:1px solid #e2e8f0;border-top:none;border-radius:0 0 14px 14px;padding:32px"">
      {corpo}
      <a href=""{link}"" style=""display:inline-block;background:#2563eb;color:#fff;text-decoration:none;font-weight:700;padding:12px 28px;border-radius:8px"">{textoBotao}</a>
      <p style=""margin:28px 0 0;font-size:12px;color:#64748b;line-height:1.6"">Se o botão não funcionar, copie e cole este endereço no navegador:<br><span style=""color:#2563eb;word-break:break-all"">{link}</span></p>
    </div>
    <p style=""text-align:center;margin:20px 0 0;font-size:12px;color:#94a3b8"">© Learnly · Plataforma de estudos para o ENEM{(string.IsNullOrEmpty(rodapeExtra) ? "" : $"<br>{rodapeExtra}")}</p>
  </div>
</body>
</html>";
    }
}
