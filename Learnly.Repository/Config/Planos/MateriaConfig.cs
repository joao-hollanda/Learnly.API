using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infra.Data.Configurations
{
    public class MateriaConfig : IEntityTypeConfiguration<Materia>
    {
        public void Configure(EntityTypeBuilder<Materia> builder)
        {
            builder.ToTable("Materias");

            builder.HasKey(m => m.MateriaId);

            builder.Property(m => m.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Cor)
                .HasMaxLength(30);

            builder.HasIndex(m => m.Nome)
                .IsUnique();

            builder.HasMany(m => m.PlanoMaterias)
                .WithOne(pm => pm.Materia)
                .HasForeignKey(pm => pm.MateriaId);
        }
    }
}
