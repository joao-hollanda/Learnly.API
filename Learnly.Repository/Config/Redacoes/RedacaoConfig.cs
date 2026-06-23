using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Redacoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Repository.Config.Redacoes
{
    public class RedacaoConfig : IEntityTypeConfiguration<Redacao>
    {
        public void Configure(EntityTypeBuilder<Redacao> builder)
        {
            builder.ToTable("Redacoes");

            builder.HasKey(r => r.RedacaoId);

            builder.Property(r => r.Tema)
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(r => r.Texto)
                   .IsRequired()
                   .HasColumnType("TEXT");

            builder.Property(r => r.ComentariosJson)
                   .IsRequired()
                   .HasColumnType("TEXT");

            builder.HasOne<Usuario>()
                   .WithMany()
                   .HasForeignKey(r => r.UsuarioId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
