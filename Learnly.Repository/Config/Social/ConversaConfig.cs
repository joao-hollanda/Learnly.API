using Learnly.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Repository.Config.Social
{
    public class ConversaConfig : IEntityTypeConfiguration<Conversa>
    {
        public void Configure(EntityTypeBuilder<Conversa> builder)
        {
            builder.ToTable("Conversas");

            builder.HasKey(c => c.ConversaId);

            builder.Property(c => c.Tipo)
                .IsRequired();

            builder.Property(c => c.DataCriacao)
                .IsRequired();

            builder.Property(c => c.UltimaMensagemData)
                .IsRequired();

            builder.HasOne(c => c.Grupo)
                .WithMany()
                .HasForeignKey(c => c.GrupoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
