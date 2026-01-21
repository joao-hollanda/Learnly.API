using Learnly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infra.Data.Configurations
{
    public class EventoEstudoConfig : IEntityTypeConfiguration<EventoEstudo>
    {
        public void Configure(EntityTypeBuilder<EventoEstudo> builder)
        {
            builder.ToTable("EventosEstudo");

            builder.HasKey(e => e.EventoEstudoId);

            builder.Property(e => e.EventoEstudoId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Titulo)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(e => e.Inicio)
                .IsRequired();

            builder.Property(e => e.Fim)
                .IsRequired();

            builder.Property(e => e.UsuarioId)
                .IsRequired();

            builder.HasIndex(e => e.UsuarioId);
        }
    }
}
