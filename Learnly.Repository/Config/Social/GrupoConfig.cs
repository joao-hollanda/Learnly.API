using Learnly.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Repository.Config.Social
{
    public class GrupoConfig : IEntityTypeConfiguration<Grupo>
    {
        public void Configure(EntityTypeBuilder<Grupo> builder)
        {
            builder.ToTable("Grupos");

            builder.HasKey(g => g.GrupoId);

            builder.Property(g => g.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(g => g.Descricao)
                .HasMaxLength(300);

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
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(g => g.Membros)
                .WithOne(m => m.Grupo)
                .HasForeignKey(m => m.GrupoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
