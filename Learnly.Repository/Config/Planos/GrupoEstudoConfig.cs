using Learnly.Domain.Entities.Planos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infra.Data.Configurations
{
    public class GrupoEstudoConfig : IEntityTypeConfiguration<GrupoEstudo>
    {
        public void Configure(EntityTypeBuilder<GrupoEstudo> builder)
        {
            builder.ToTable("GruposEstudo");

            builder.HasKey(g => g.GrupoId);

            builder.Property(g => g.Chave)
                .IsRequired()
                .HasMaxLength(12);

            builder.HasIndex(g => g.Chave)
                .IsUnique();

            builder.Property(g => g.DataCriacao)
                .IsRequired();

            builder.HasOne(g => g.Criador)
                .WithMany()
                .HasForeignKey(g => g.CriadorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(g => g.Planos)
                .WithOne(p => p.Grupo)
                .HasForeignKey(p => p.GrupoId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
