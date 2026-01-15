using Learnly.Domain.Entities.Simulados;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Repository.Config.Simulados
{
       public class AlternativaConfig : IEntityTypeConfiguration<Alternativa>
       {
              public void Configure(EntityTypeBuilder<Alternativa> builder)
              {
                     builder.ToTable("Alternativas", table =>
                     {
                            table.HasCheckConstraint(
                       "CK_Alternativa_TextoOuArquivo",
                       "Texto IS NOT NULL OR Arquivo IS NOT NULL"
                   );
                     });

                     builder.HasKey(a => a.AlternativaId);

                     builder.Property(a => a.Letra)
                            .IsRequired()
                            .HasMaxLength(1);

                     builder.Property(a => a.Texto)
                            .IsRequired(false);

                     builder.Property(a => a.Arquivo)
                            .IsRequired(false);

                     builder.Property(a => a.Correta)
                            .IsRequired();

                     builder.HasOne(a => a.Questao)
                            .WithMany(q => q.Alternativas)
                            .HasForeignKey(a => a.QuestaoId)
                            .OnDelete(DeleteBehavior.Cascade);
              }
       }
}
