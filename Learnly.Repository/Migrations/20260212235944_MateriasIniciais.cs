using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Learnly.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MateriasIniciais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Materias",
                columns: new[] { "MateriaId", "Cor", "GeradaPorIA", "Nome" },
                values: new object[,]
                {
                    { 1, "#1ABC9C", false, "Português" },
                    { 2, "#16A085", false, "Literatura" },
                    { 3, "#2ECC71", false, "Redação" },
                    { 4, "#27AE60", false, "Inglês" },
                    { 5, "#229954", false, "Espanhol" },
                    { 6, "#3498DB", false, "Matemática" },
                    { 7, "#2E86C1", false, "Matemática Financeira" },
                    { 8, "#2874A6", false, "Raciocínio Lógico" },
                    { 9, "#1F618D", false, "Estatística" },
                    { 10, "#9B59B6", false, "Física" },
                    { 11, "#8E44AD", false, "Química" },
                    { 12, "#7D3C98", false, "Biologia" },
                    { 13, "#E67E22", false, "História" },
                    { 14, "#D35400", false, "Geografia" },
                    { 15, "#CA6F1E", false, "Filosofia" },
                    { 16, "#BA4A00", false, "Sociologia" },
                    { 17, "#C0392B", false, "Direito Constitucional" },
                    { 18, "#A93226", false, "Direito Administrativo" },
                    { 19, "#922B21", false, "Direito Penal" },
                    { 20, "#7B241C", false, "Direito Civil" },
                    { 21, "#5D6D7E", false, "Informática" },
                    { 22, "#34495E", false, "Algoritmos" },
                    { 23, "#2C3E50", false, "Estrutura de Dados" },
                    { 24, "#212F3D", false, "Banco de Dados" },
                    { 25, "#1B2631", false, "Programação" },
                    { 26, "#17202A", false, "Engenharia de Software" },
                    { 27, "#F4D03F", false, "Atualidades" },
                    { 28, "#F39C12", false, "Interpretação de Texto" },
                    { 29, "#E59866", false, "Conhecimentos Gerais" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Materias",
                keyColumn: "MateriaId",
                keyValue: 29);
        }
    }
}
