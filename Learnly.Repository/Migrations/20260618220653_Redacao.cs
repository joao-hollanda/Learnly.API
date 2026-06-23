using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Learnly.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Redacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Redacoes",
                columns: table => new
                {
                    RedacaoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Tema = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Texto = table.Column<string>(type: "TEXT", nullable: false),
                    NotaC1 = table.Column<int>(type: "integer", nullable: false),
                    NotaC2 = table.Column<int>(type: "integer", nullable: false),
                    NotaC3 = table.Column<int>(type: "integer", nullable: false),
                    NotaC4 = table.Column<int>(type: "integer", nullable: false),
                    NotaC5 = table.Column<int>(type: "integer", nullable: false),
                    NotaFinal = table.Column<int>(type: "integer", nullable: false),
                    ComentariosJson = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redacoes", x => x.RedacaoId);
                    table.ForeignKey(
                        name: "FK_Redacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Redacoes_UsuarioId",
                table: "Redacoes",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Redacoes");
        }
    }
}
