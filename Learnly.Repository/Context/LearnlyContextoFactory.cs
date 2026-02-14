using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

public class LearnlyContextoFactory : IDesignTimeDbContextFactory<LearnlyContexto>
{
    public LearnlyContexto CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<LearnlyContexto>();
        optionsBuilder.UseNpgsql(connectionString);

        return new LearnlyContexto(optionsBuilder.Options);
    }
}