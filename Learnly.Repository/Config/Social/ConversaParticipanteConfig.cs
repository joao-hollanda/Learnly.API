using Learnly.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Repository.Config.Social
{
    public class ConversaParticipanteConfig : IEntityTypeConfiguration<ConversaParticipante>
    {
        public void Configure(EntityTypeBuilder<ConversaParticipante> builder)
        {
            builder.ToTable("ConversaParticipantes");

            builder.HasKey(p => p.ConversaParticipanteId);

            builder.HasOne(p => p.Conversa)
                .WithMany(c => c.Participantes)
                .HasForeignKey(p => p.ConversaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => new { p.ConversaId, p.UsuarioId })
                .IsUnique();
        }
    }
}
