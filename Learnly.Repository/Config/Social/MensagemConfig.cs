using Learnly.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Repository.Config.Social
{
    public class MensagemConfig : IEntityTypeConfiguration<Mensagem>
    {
        public void Configure(EntityTypeBuilder<Mensagem> builder)
        {
            builder.ToTable("Mensagens");

            builder.HasKey(m => m.MensagemId);

            builder.Property(m => m.Texto)
                .HasMaxLength(4000);

            builder.Property(m => m.Tipo)
                .IsRequired();

            builder.Property(m => m.AnexoPayload)
                .HasColumnType("jsonb");

            builder.Property(m => m.DataEnvio)
                .IsRequired();

            builder.HasOne(m => m.Conversa)
                .WithMany(c => c.Mensagens)
                .HasForeignKey(m => m.ConversaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Remetente)
                .WithMany()
                .HasForeignKey(m => m.RemetenteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(m => new { m.ConversaId, m.DataEnvio });
        }
    }
}
