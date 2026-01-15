using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infra.Data.Configurations
{
    public class PlanoMateriaConfig : IEntityTypeConfiguration<PlanoMateria>
    {
        public void Configure(EntityTypeBuilder<PlanoMateria> builder)
        {
            builder.ToTable("PlanoMaterias");

            builder.HasKey(pm => pm.PlanoMateriaId);

            builder.Property(pm => pm.HorasTotais)
                .IsRequired();

            builder.Property(pm => pm.HorasConcluidas)
                .IsRequired();

            builder.HasOne(pm => pm.Plano)
                .WithMany(p => p.PlanoMaterias)
                .HasForeignKey(pm => pm.PlanoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pm => pm.Materia)
                .WithMany(m => m.PlanoMaterias)
                .HasForeignKey(pm => pm.MateriaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pm => new { pm.PlanoId, pm.MateriaId })
                .IsUnique();
        }
    }
}
