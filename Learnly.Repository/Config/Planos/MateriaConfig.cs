using Learnly.Domain.Entities;
using Learnly.Domain.Entities.Planos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infra.Data.Configurations
{
    public class MateriaConfig : IEntityTypeConfiguration<Materia>
    {
        public void Configure(EntityTypeBuilder<Materia> builder)
        {
            builder.ToTable("Materias");

            builder.HasKey(m => m.MateriaId);

            builder.HasIndex(m => m.Nome).IsUnique();

            builder.Property(m => m.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.GeradaPorIA);

            builder.Property(m => m.Cor)
                .HasMaxLength(30);

            builder.HasMany(m => m.PlanoMaterias)
                .WithOne(pm => pm.Materia)
                .HasForeignKey(pm => pm.MateriaId);

            builder.HasData(

new Materia { MateriaId = 1, Nome = "Português", GeradaPorIA = false, Cor = "#1ABC9C" },
new Materia { MateriaId = 2, Nome = "Literatura", GeradaPorIA = false, Cor = "#16A085" },
new Materia { MateriaId = 3, Nome = "Redação", GeradaPorIA = false, Cor = "#2ECC71" },
new Materia { MateriaId = 4, Nome = "Inglês", GeradaPorIA = false, Cor = "#27AE60" },
new Materia { MateriaId = 5, Nome = "Espanhol", GeradaPorIA = false, Cor = "#229954" },

new Materia { MateriaId = 6, Nome = "Matemática", GeradaPorIA = false, Cor = "#3498DB" },
new Materia { MateriaId = 7, Nome = "Matemática Financeira", GeradaPorIA = false, Cor = "#2E86C1" },
new Materia { MateriaId = 8, Nome = "Raciocínio Lógico", GeradaPorIA = false, Cor = "#2874A6" },
new Materia { MateriaId = 9, Nome = "Estatística", GeradaPorIA = false, Cor = "#1F618D" },

new Materia { MateriaId = 10, Nome = "Física", GeradaPorIA = false, Cor = "#9B59B6" },
new Materia { MateriaId = 11, Nome = "Química", GeradaPorIA = false, Cor = "#8E44AD" },
new Materia { MateriaId = 12, Nome = "Biologia", GeradaPorIA = false, Cor = "#7D3C98" },

new Materia { MateriaId = 13, Nome = "História", GeradaPorIA = false, Cor = "#E67E22" },
new Materia { MateriaId = 14, Nome = "Geografia", GeradaPorIA = false, Cor = "#D35400" },
new Materia { MateriaId = 15, Nome = "Filosofia", GeradaPorIA = false, Cor = "#CA6F1E" },
new Materia { MateriaId = 16, Nome = "Sociologia", GeradaPorIA = false, Cor = "#BA4A00" },

new Materia { MateriaId = 17, Nome = "Direito Constitucional", GeradaPorIA = false, Cor = "#C0392B" },
new Materia { MateriaId = 18, Nome = "Direito Administrativo", GeradaPorIA = false, Cor = "#A93226" },
new Materia { MateriaId = 19, Nome = "Direito Penal", GeradaPorIA = false, Cor = "#922B21" },
new Materia { MateriaId = 20, Nome = "Direito Civil", GeradaPorIA = false, Cor = "#7B241C" },
new Materia { MateriaId = 21, Nome = "Informática", GeradaPorIA = false, Cor = "#5D6D7E" },

new Materia { MateriaId = 22, Nome = "Algoritmos", GeradaPorIA = false, Cor = "#34495E" },
new Materia { MateriaId = 23, Nome = "Estrutura de Dados", GeradaPorIA = false, Cor = "#2C3E50" },
new Materia { MateriaId = 24, Nome = "Banco de Dados", GeradaPorIA = false, Cor = "#212F3D" },
new Materia { MateriaId = 25, Nome = "Programação", GeradaPorIA = false, Cor = "#1B2631" },
new Materia { MateriaId = 26, Nome = "Engenharia de Software", GeradaPorIA = false, Cor = "#17202A" },

new Materia { MateriaId = 27, Nome = "Atualidades", GeradaPorIA = false, Cor = "#F4D03F" },
new Materia { MateriaId = 28, Nome = "Interpretação de Texto", GeradaPorIA = false, Cor = "#F39C12" },
new Materia { MateriaId = 29, Nome = "Conhecimentos Gerais", GeradaPorIA = false, Cor = "#E59866" }
);
        }
    }
}
