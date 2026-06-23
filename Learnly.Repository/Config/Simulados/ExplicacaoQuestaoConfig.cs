using Learnly.Domain.Entities.Simulados;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Repository.Config.Simulados
{
    public class ExplicacaoQuestaoConfig : IEntityTypeConfiguration<ExplicacaoQuestao>
    {
        public void Configure(EntityTypeBuilder<ExplicacaoQuestao> builder)
        {
            builder.ToTable("ExplicacoesQuestao");

            builder.HasKey(e => e.QuestaoId);

            builder.Property(e => e.QuestaoId)
                   .ValueGeneratedNever();

            builder.Property(e => e.Explicacao)
                   .IsRequired()
                   .HasColumnType("TEXT");

            builder.HasOne<Questao>()
                   .WithMany()
                   .HasForeignKey(e => e.QuestaoId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
