
using Learnly.Repository.Seed;
using Microsoft.EntityFrameworkCore;

class Program
{
    static async Task Main(string[] args)
    {
        var dbPath = "Learnly.db";

        var options = new DbContextOptionsBuilder<LearnlyContexto>()
            .UseSqlite($"Data Source={dbPath}")
            .EnableSensitiveDataLogging() // ajuda no debug
            .Options;

        using var contexto = new LearnlyContexto(options);

        Console.WriteLine("Garantindo banco de dados...");
        await contexto.Database.EnsureCreatedAsync();

        Console.WriteLine("Banco criado ou já existente.");

        Console.WriteLine("Iniciando seed das questões ENEM...");
        await EnemSeeder.SeedAsync(contexto);

        Console.WriteLine("Seed finalizado com sucesso!");
    }
}
