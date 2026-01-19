using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EventoEstudoConfig : IEntityTypeConfiguration<EventoEstudo>
{
    public void Configure(EntityTypeBuilder<EventoEstudo> builder)
    {
        builder.ToTable("EventosEstudo");

        builder.HasKey(e => e.EventoId);

        builder.Property(e => e.Titulo)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(e => e.Inicio)
            .IsRequired();

        builder.Property(e => e.Fim)
            .IsRequired();

        builder.Property(e => e.PlanoId)
            .IsRequired();
    }
}
