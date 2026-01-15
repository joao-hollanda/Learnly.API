using Learnly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infra.Data.Configurations
{
       public class PlanoEstudoConfig : IEntityTypeConfiguration<PlanoEstudo>
       {
              public void Configure(EntityTypeBuilder<PlanoEstudo> builder)
              {
                     builder.ToTable("PlanosEstudo");

                     builder.HasKey(p => p.PlanoId);

                     builder.Property(p => p.Titulo)
                         .IsRequired()
                         .HasMaxLength(150);

                     builder.Property(p => p.Objetivo)
                         .IsRequired()
                         .HasMaxLength(300);

                     builder.Property(p => p.DataInicio)
                         .IsRequired();

                     builder.Property(p => p.DataFim)
                         .IsRequired();

                     builder.Property(p => p.HorasPorSemana)
                         .IsRequired();

                     builder.Property(p => p.Ativo)
                         .IsRequired();

                     builder.HasOne(p => p.Usuario)
                         .WithMany(u => u.PlanoEstudo)
                         .HasForeignKey(p => p.UsuarioId)
                         .OnDelete(DeleteBehavior.Cascade);

                     builder.HasMany(p => p.PlanoMaterias)
                         .WithOne(pm => pm.Plano)
                         .HasForeignKey(pm => pm.PlanoId);
              }
       }
}
