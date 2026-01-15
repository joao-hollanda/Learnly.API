using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Learnly.Domain.Entities;

namespace Learnly.Repository.Config
{
       public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
       {
              public void Configure(EntityTypeBuilder<Usuario> builder)
              {
                     builder.ToTable("Usuarios");

                     builder.HasKey(u => u.Id);

                     builder.Property(u => u.Nome)
                            .IsRequired()
                            .HasMaxLength(100);

                     builder.Property(u => u.Email)
                            .IsRequired()
                            .HasMaxLength(150);

                     builder.Property(u => u.Senha)
                            .IsRequired();

                     builder.HasMany(u => u.PlanoEstudo)
                            .WithOne(p => p.Usuario)
                            .HasForeignKey(p => p.UsuarioId)
                            .OnDelete(DeleteBehavior.Cascade);
              }
       }
}
