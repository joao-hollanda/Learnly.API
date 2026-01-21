using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnly.Repository.Migrations
{
    /// <inheritdoc />
    public partial class eventos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventosEstudo_PlanosEstudo_PlanoId",
                table: "EventosEstudo");

            migrationBuilder.RenameColumn(
                name: "PlanoId",
                table: "EventosEstudo",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "EventoId",
                table: "EventosEstudo",
                newName: "EventoEstudoId");

            migrationBuilder.RenameIndex(
                name: "IX_EventosEstudo_PlanoId",
                table: "EventosEstudo",
                newName: "IX_EventosEstudo_UsuarioId");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "EventosEstudo",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "EventosEstudo",
                newName: "PlanoId");

            migrationBuilder.RenameColumn(
                name: "EventoEstudoId",
                table: "EventosEstudo",
                newName: "EventoId");

            migrationBuilder.RenameIndex(
                name: "IX_EventosEstudo_UsuarioId",
                table: "EventosEstudo",
                newName: "IX_EventosEstudo_PlanoId");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "EventosEstudo",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddForeignKey(
                name: "FK_EventosEstudo_PlanosEstudo_PlanoId",
                table: "EventosEstudo",
                column: "PlanoId",
                principalTable: "PlanosEstudo",
                principalColumn: "PlanoId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
