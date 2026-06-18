using Learnly.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Repository.Config.Social
{
    public class AmizadeConfig : IEntityTypeConfiguration<Amizade>
    {
        public void Configure(EntityTypeBuilder<Amizade> builder)
        {
            builder.ToTable("Amizades");

            builder.HasKey(a => a.AmizadeId);

            builder.Property(a => a.Status)
                .IsRequired();

            builder.Property(a => a.DataSolicitacao)
                .IsRequired();

            builder.HasOne(a => a.Solicitante)
                .WithMany()
                .HasForeignKey(a => a.SolicitanteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Destinatario)
                .WithMany()
                .HasForeignKey(a => a.DestinatarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => new { a.SolicitanteId, a.DestinatarioId })
                .IsUnique();
        }
    }
}
