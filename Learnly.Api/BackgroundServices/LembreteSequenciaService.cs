using Learnly.Application.Interfaces;
using Learnly.Repository.Interfaces;

namespace Learnly.Api.BackgroundServices
{
    public class LembreteSequenciaService : BackgroundService
    {
        private const int SequenciaMinima = 2;
        private static readonly TimeSpan HorarioEnvioUtc = TimeSpan.FromHours(22);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LembreteSequenciaService> _logger;

        public LembreteSequenciaService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<LembreteSequenciaService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var agora = DateTime.UtcNow;
                var proximo = agora.Date.Add(HorarioEnvioUtc);
                if (proximo <= agora) proximo = proximo.AddDays(1);

                try
                {
                    await Task.Delay(proximo - agora, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                try
                {
                    await EnviarLembretesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao processar lembretes de sequência.");
                }
            }
        }

        private async Task EnviarLembretesAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var usuarioRepo = scope.ServiceProvider.GetRequiredService<IUsuarioRepositorio>();
            var horaRepo = scope.ServiceProvider.GetRequiredService<IHoraLancadaRepositorio>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var loginAplicacao = scope.ServiceProvider.GetRequiredService<ILoginAplicacao>();

            var frontendUrl = _configuration["Email:FrontendUrl"] ?? "https://www.learnly.com.br";
            var apiUrl = _configuration["Email:ApiUrl"] ?? "http://localhost:5080";

            var ontem = DateTime.UtcNow.AddHours(-3).Date.AddDays(-1);

            var usuarios = await usuarioRepo.ListarParaLembreteEmail();

            foreach (var usuario in usuarios)
            {
                if (ct.IsCancellationRequested) break;

                var datas = await horaRepo.ListarDatasComLancamento(usuario.Id);
                var sequencia = SequenciaAteOntem(datas, ontem);
                if (sequencia < SequenciaMinima) continue;

                try
                {
                    var token = loginAplicacao.GerarTokenAcao(usuario.Id, usuario.Email, "unsubscribe", TimeSpan.FromDays(365));
                    var linkDescadastro = $"{apiUrl}/api/login/descadastrar-emails?token={Uri.EscapeDataString(token)}";

                    await emailService.EnviarLembreteSequenciaAsync(
                        usuario.Email, usuario.Nome, sequencia, $"{frontendUrl}/home", linkDescadastro);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao enviar lembrete de sequência para {Email}", usuario.Email);
                }
            }
        }

        private static int SequenciaAteOntem(List<DateTime> datas, DateTime ontem)
        {
            var dias = datas.Select(d => d.Date).Distinct().OrderByDescending(d => d).ToList();
            if (dias.Count == 0 || dias[0] != ontem) return 0;

            var sequencia = 1;
            var referencia = dias[0];
            for (var i = 1; i < dias.Count; i++)
            {
                if (dias[i] == referencia.AddDays(-1))
                {
                    sequencia++;
                    referencia = dias[i];
                }
                else break;
            }
            return sequencia;
        }
    }
}
