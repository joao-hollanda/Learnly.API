using Microsoft.EntityFrameworkCore;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Simulados;
using Learnly.Repository.Config;
using Learnly.Infra.Data.Configurations;
using Learnly.Repository.Config.Simulados;
using Learnly.Domain.Entities.Planos;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class LearnlyContexto : DbContext
{
    public DbSet<PlanoEstudo> PlanosEstudo { get; set; }
    public DbSet<Materia> Materias { get; set; }
    public DbSet<PlanoMateria> PlanoMateria { get; set; }
    public DbSet<HoraLancada> HorasLancadas { get; set; }
    public DbSet<EventoEstudo> EventosEstudo { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Simulado> Simulados { get; set; }
    public DbSet<Questao> Questoes { get; set; }
    public DbSet<SimuladoQuestao> SimuladoQuestoes { get; set; }
    public DbSet<RespostaSimulado> RespostasSimulado { get; set; }
    public DbSet<Alternativa> Alternativas { get; set; }


    public LearnlyContexto(DbContextOptions<LearnlyContexto> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AlternativaConfig());
        modelBuilder.ApplyConfiguration(new UsuarioConfig());
        modelBuilder.ApplyConfiguration(new SimuladoConfig());
        modelBuilder.ApplyConfiguration(new QuestaoConfig());
        modelBuilder.ApplyConfiguration(new SimuladoQuestaoConfig());
        modelBuilder.ApplyConfiguration(new RespostaSimuladoConfig());
        modelBuilder.ApplyConfiguration(new PlanoEstudoConfig());
        modelBuilder.ApplyConfiguration(new MateriaConfig());
        modelBuilder.ApplyConfiguration(new PlanoMateriaConfig());
        modelBuilder.ApplyConfiguration(new EventoEstudoConfig());
        modelBuilder.ApplyConfiguration(new HoraLancadaConfig());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(
                        new ValueConverter<DateTime, DateTime>(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        )
                    );
                }
            }
        }
    }
}
