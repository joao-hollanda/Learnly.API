using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

public class LearnlyContextoFactory : IDesignTimeDbContextFactory<LearnlyContexto>
{
    public LearnlyContexto CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
        {
            var apiPath = Path.Combine(basePath, "Learnly.Api");
            if (Directory.Exists(apiPath))
                basePath = apiPath;
            else
                basePath = Path.Combine(basePath, "..", "Learnly.Api");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets("1a7d4dec-1037-4cb6-8caf-302fe91ab02d")
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<LearnlyContexto>();
        optionsBuilder.UseNpgsql(connectionString);

        return new LearnlyContexto(optionsBuilder.Options);
    }
}