using Learnly.Repository.Seed;
using Microsoft.EntityFrameworkCore;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString =
            "Host=dpg-d5ppnan5c7fs73bthqr0-a.virginia-postgres.render.com;" +
            "Port=5432;" +
            "Database=learnly_db;" +
            "Username=learnly_db_user;" +
            "Password=rZ77YlBq2j6UE4CRoAr6CP5ezF0BX6Bo;" +
            "SSL Mode=Require;" +
            "Trust Server Certificate=true";

        var options = new DbContextOptionsBuilder<LearnlyContexto>()
            .UseNpgsql(connectionString)   // 🔥 Postgres correto
            .EnableSensitiveDataLogging()
            .Options;

        using var contexto = new LearnlyContexto(options);

        Console.WriteLine("Aplicando migrations no banco...");
        await contexto.Database.MigrateAsync();   // 🔥 aplica migrations automaticamente

        Console.WriteLine("Iniciando seed das questões ENEM...");
        await EnemSeeder.SeedAsync(contexto);

        Console.WriteLine("Seed finalizado com sucesso!");
    }
}
