using Microsoft.EntityFrameworkCore;
using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Simulados;
using Learnly.Repository.Config;
using Learnly.Infra.Data.Configurations;
using Learnly.Repository.Config.Simulados;
using Learnly.Domain.Entities.Planos;

public class LearnlyContexto : DbContext
{
    private readonly DbContextOptions _options;
    public DbSet<PlanoEstudo> PlanosEstudo { get; set; }
    public DbSet<Materia> Materias { get; set; }
    public DbSet<PlanoMateria> PlanoMateria { get; set; }
    public DbSet<EventoEstudo> Eventos { get; set;}
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Simulado> Simulados { get; set; }
    public DbSet<Questao> Questoes { get; set; }
    public DbSet<SimuladoQuestao> SimuladoQuestoes { get; set; }
    public DbSet<RespostaSimulado> RespostasSimulado { get; set; }
    public DbSet<Alternativa> Alternativas { get; set; }

    public LearnlyContexto()
    { }

    public LearnlyContexto(DbContextOptions options) : base(options)
    {
        _options = options;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_options == null)
            optionsBuilder.UseSqlite(@"Data Source=./Learnly.db;");
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
    }
}
