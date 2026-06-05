using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Learnly.Domain.Entities.Simulados;

namespace Learnly.Repository.Config
{
    public class SimuladoConfig : IEntityTypeConfiguration<Simulado>
    {
        public void Configure(EntityTypeBuilder<Simulado> builder)
        {
            builder.ToTable("Simulados");

            builder.HasKey(s => s.SimuladoId);

            builder.HasMany(s => s.Questoes)
                   .WithOne(sq => sq.Simulado)
                   .HasForeignKey(sq => sq.SimuladoId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Usuario)
                   .WithMany(u => u.Simulados);

            builder.OwnsOne(s => s.Desempenho);

            builder.OwnsMany(s => s.MateriaisRecomendados, m =>
            {
                m.ToTable("MateriaisRecomendados");
                m.WithOwner().HasForeignKey("SimuladoId");
            });
        }
    }
}
