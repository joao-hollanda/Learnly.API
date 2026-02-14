using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class LearnlyContextoFactory : IDesignTimeDbContextFactory<LearnlyContexto>
{
    public LearnlyContexto CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LearnlyContexto>();

        optionsBuilder.UseNpgsql("Host=dpg-d5ppnan5c7fs73bthqr0-a.oregon-postgres.render.com;Port=5432;Database=learnly_db;Username=learnly_db_user;Password=rZ77YlBq2j6UE4CRoAr6CP5ezF0BX6Bo;SSL Mode=Require;Trust Server Certificate=true"
);

        return new LearnlyContexto(optionsBuilder.Options);
    }
}