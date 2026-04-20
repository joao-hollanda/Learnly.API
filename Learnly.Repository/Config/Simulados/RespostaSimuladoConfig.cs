using Learnly.Domain.Entities.Simulados;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Repository.Config
{
    public class RespostaSimuladoConfig : IEntityTypeConfiguration<RespostaSimulado>
    {
        public void Configure(EntityTypeBuilder<RespostaSimulado> builder)
        {
            builder.ToTable("RespostasSimulado");

            builder.HasKey(r => r.RespostaId);

            builder.Property(r => r.Explicacao)
                   .IsRequired(false);
                   
            // Relacionamento com Simulado
            builder.HasOne(r => r.Simulado)
                   .WithMany(s => s.Respostas)
                   .HasForeignKey(r => r.SimuladoId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Questao)
                   .WithMany(q => q.Respostas)
                   .HasForeignKey(r => r.QuestaoId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Alternativa)
                   .WithMany()
                   .HasForeignKey(r => r.AlternativaId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
