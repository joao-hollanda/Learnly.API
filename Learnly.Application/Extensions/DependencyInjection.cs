using Learnly.Application.Applications;
using Learnly.Application.Interfaces;
using Learnly.Repository;
using Learnly.Repository.Interfaces;
using Learnly.Repository.Repositories;
using Learnly.Services.IAService;
using Learnly.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Learnly.Application.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<ILoginAplicacao, LoginAplicacao>();
            services.AddScoped<IUsuarioAplicacao, UsuarioAplicacao>();
            services.AddScoped<ISimuladoAplicacao, SimuladoAplicacao>();
            services.AddScoped<IPlanoAplicacao, PlanoAplicacao>();
            services.AddScoped<IMateriaAplicacao, MateriaAplicacao>();
            services.AddScoped<IEventoEstudoAplicacao, EventoEstudoAplicacao>();
            services.AddScoped<IDesempenhoAplicacao, DesempenhoAplicacao>();

            services.AddScoped<IIAService, IAService>();
            services.AddScoped<IIAAplicacao, IAAplicacao>();

            services.AddScoped<IRedacaoIAService, RedacaoIAService>();
            services.AddScoped<IRedacaoAplicacao, RedacaoAplicacao>();

            services.AddScoped<IAmizadeAplicacao, AmizadeAplicacao>();
            services.AddScoped<IGrupoAplicacao, GrupoAplicacao>();
            services.AddScoped<IChatAplicacao, ChatAplicacao>();

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
            services.AddScoped<ISimuladoRepositorio, SimuladoRepositorio>();
            services.AddScoped<IExplicacaoQuestaoRepositorio, ExplicacaoQuestaoRepositorio>();
            services.AddScoped<IRedacaoRepositorio, RedacaoRepositorio>();
            services.AddScoped<IPlanoRepositorio, PlanoRepositorio>();
            services.AddScoped<IMateriaRepositorio, MateriaRepositorio>();
            services.AddScoped<IHoraLancadaRepositorio, HoraLancadaRepositorio>();
            services.AddScoped<IEventoEstudoRepositorio, EventoEstudoRepositorio>();
            services.AddScoped<IAmizadeRepositorio, AmizadeRepositorio>();
            services.AddScoped<IGrupoRepositorio, GrupoRepositorio>();
            services.AddScoped<IChatRepositorio, ChatRepositorio>();

            services.AddDbContext<LearnlyContexto>(options =>
                options.UseNpgsql(
                    config.GetConnectionString("DefaultConnection"),
                    npgsql => npgsql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null
                    )
                )
            );

            return services;
        }
    }
}
