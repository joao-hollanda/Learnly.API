using Learnly.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Repository.Config.Social
{
    public class GrupoMembroConfig : IEntityTypeConfiguration<GrupoMembro>
    {
        public void Configure(EntityTypeBuilder<GrupoMembro> builder)
        {
            builder.ToTable("GrupoMembros");

            builder.HasKey(m => m.GrupoMembroId);

            builder.Property(m => m.Papel)
                .IsRequired();

            builder.Property(m => m.DataEntrada)
                .IsRequired();

            builder.HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => new { m.GrupoId, m.UsuarioId })
                .IsUnique();
        }
    }
}
