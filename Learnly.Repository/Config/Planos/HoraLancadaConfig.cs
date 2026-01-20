using Learnly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Repository.Config
{
    public class HoraLancadaConfig : IEntityTypeConfiguration<HoraLancada>
    {
        public void Configure(EntityTypeBuilder<HoraLancada> builder)
        {
            builder.ToTable("HorasLancadas");

            builder.HasKey(h => h.HoraLancadaId);

            builder.Property(h => h.QuantdadeHoras)
                   .IsRequired();

            builder.Property(h => h.Data)
                   .IsRequired();

            builder.HasOne(h => h.Usuario)
                   .WithMany()
                   .HasForeignKey(h => h.UsuarioId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(h => new { h.UsuarioId, h.Data });
        }
    }
}
