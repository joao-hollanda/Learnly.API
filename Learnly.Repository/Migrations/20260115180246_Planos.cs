using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnly.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Planos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlanosEstudo_UsuarioId",
                table: "PlanosEstudo");

            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PlanoEstudoId",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "StatusPlano",
                table: "PlanosEstudo",
                newName: "Ativo");

            migrationBuilder.AlterColumn<int>(
                name: "SimuladoId",
                table: "Simulados",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "PlanosEstudo",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    MateriaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cor = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.MateriaId);
                });

            migrationBuilder.CreateTable(
                name: "PlanoMaterias",
                columns: table => new
                {
                    PlanoMateriaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlanoId = table.Column<int>(type: "INTEGER", nullable: false),
                    MateriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    HorasTotais = table.Column<int>(type: "INTEGER", nullable: false),
                    HorasConcluidas = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanoMaterias", x => x.PlanoMateriaId);
                    table.ForeignKey(
                        name: "FK_PlanoMaterias_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "MateriaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanoMaterias_PlanosEstudo_PlanoId",
                        column: x => x.PlanoId,
                        principalTable: "PlanosEstudo",
                        principalColumn: "PlanoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanosEstudo_UsuarioId",
                table: "PlanosEstudo",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Materias_Nome",
                table: "Materias",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanoMaterias_MateriaId",
                table: "PlanoMaterias",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanoMaterias_PlanoId_MateriaId",
                table: "PlanoMaterias",
                columns: new[] { "PlanoId", "MateriaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanoMaterias");

            migrationBuilder.DropTable(
                name: "Materias");

            migrationBuilder.DropIndex(
                name: "IX_PlanosEstudo_UsuarioId",
                table: "PlanosEstudo");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "PlanosEstudo");

            migrationBuilder.RenameColumn(
                name: "Ativo",
                table: "PlanosEstudo",
                newName: "StatusPlano");

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "Usuarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlanoEstudoId",
                table: "Usuarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "SimuladoId",
                table: "Simulados",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanosEstudo_UsuarioId",
                table: "PlanosEstudo",
                column: "UsuarioId",
                unique: true);
        }
    }
}
