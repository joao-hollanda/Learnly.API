using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Learnly.Repository.Migrations
{
    /// <inheritdoc />
    public partial class CompartilhamentoPlanos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GrupoId",
                table: "PlanosEstudo",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GruposEstudo",
                columns: table => new
                {
                    GrupoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Chave = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    CriadorId = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposEstudo", x => x.GrupoId);
                    table.ForeignKey(
                        name: "FK_GruposEstudo_Usuarios_CriadorId",
                        column: x => x.CriadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanosEstudo_GrupoId",
                table: "PlanosEstudo",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposEstudo_Chave",
                table: "GruposEstudo",
                column: "Chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GruposEstudo_CriadorId",
                table: "GruposEstudo",
                column: "CriadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanosEstudo_GruposEstudo_GrupoId",
                table: "PlanosEstudo",
                column: "GrupoId",
                principalTable: "GruposEstudo",
                principalColumn: "GrupoId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanosEstudo_GruposEstudo_GrupoId",
                table: "PlanosEstudo");

            migrationBuilder.DropTable(
                name: "GruposEstudo");

            migrationBuilder.DropIndex(
                name: "IX_PlanosEstudo_GrupoId",
                table: "PlanosEstudo");

            migrationBuilder.DropColumn(
                name: "GrupoId",
                table: "PlanosEstudo");
        }
    }
}
