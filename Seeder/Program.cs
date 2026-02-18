using Learnly.Repository.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

class Program
{
    static string GetAppSettingsBasePath()
    {
        var basePath = Directory.GetCurrentDirectory();
        if (File.Exists(Path.Combine(basePath, "appsettings.json")))
            return basePath;
        var apiPath = Path.Combine(basePath, "Learnly.Api");
        if (Directory.Exists(apiPath))
            return apiPath;
        return Path.Combine(basePath, "..", "Learnly.Api");
    }

    static async Task Main(string[] args)
    {
        var basePath = GetAppSettingsBasePath();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");

        var options = new DbContextOptionsBuilder<LearnlyContexto>()
            .UseNpgsql(connectionString)
            .EnableSensitiveDataLogging()
            .Options;

        using var contexto = new LearnlyContexto(options);

        Console.WriteLine("Aplicando migrations no banco...");
        await contexto.Database.MigrateAsync(); 

        Console.WriteLine("Iniciando seed das questões ENEM...");
        await EnemSeeder.SeedAsync(contexto);

        Console.WriteLine("Seed finalizado com sucesso!");
    }
}
