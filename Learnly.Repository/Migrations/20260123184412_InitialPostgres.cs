using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Learnly.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventosEstudo",
                columns: table => new
                {
                    EventoEstudoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosEstudo", x => x.EventoEstudoId);
                });

            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    MateriaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.MateriaId);
                });

            migrationBuilder.CreateTable(
                name: "Questoes",
                columns: table => new
                {
                    QuestaoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Disciplina = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Lingua = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Contexto = table.Column<string>(type: "TEXT", nullable: true),
                    Arquivos = table.Column<string>(type: "TEXT", nullable: true),
                    IntroducaoAlternativa = table.Column<string>(type: "text", nullable: true),
                    AlternativaCorreta = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questoes", x => x.QuestaoId);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Senha = table.Column<string>(type: "text", nullable: false),
                    DataCriacao = table.Column<DateOnly>(type: "date", nullable: false),
                    StatusConta = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alternativas",
                columns: table => new
                {
                    AlternativaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestaoId = table.Column<int>(type: "integer", nullable: false),
                    Letra = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    Texto = table.Column<string>(type: "text", nullable: true),
                    Arquivo = table.Column<string>(type: "text", nullable: true),
                    Correta = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alternativas", x => x.AlternativaId);
                    table.CheckConstraint(
    "CK_Alternativa_TextoOuArquivo",
    "\"Texto\" IS NOT NULL OR \"Arquivo\" IS NOT NULL");

                    table.ForeignKey(
                        name: "FK_Alternativas_Questoes_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questoes",
                        principalColumn: "QuestaoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HorasLancadas",
                columns: table => new
                {
                    HoraLancadaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    QuantdadeHoras = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorasLancadas", x => x.HoraLancadaId);
                    table.ForeignKey(
                        name: "FK_HorasLancadas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanosEstudo",
                columns: table => new
                {
                    PlanoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Objetivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HorasPorSemana = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosEstudo", x => x.PlanoId);
                    table.ForeignKey(
                        name: "FK_PlanosEstudo_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Simulados",
                columns: table => new
                {
                    SimuladoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    NotaFinal = table.Column<decimal>(type: "numeric", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Desempenho_QuantidadeDeQuestoes = table.Column<int>(type: "integer", nullable: true),
                    Desempenho_QuantidadeDeAcertos = table.Column<int>(type: "integer", nullable: true),
                    Desempenho_Feedback = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Simulados", x => x.SimuladoId);
                    table.ForeignKey(
                        name: "FK_Simulados_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanoMaterias",
                columns: table => new
                {
                    PlanoMateriaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanoId = table.Column<int>(type: "integer", nullable: false),
                    MateriaId = table.Column<int>(type: "integer", nullable: false),
                    HorasTotais = table.Column<int>(type: "integer", nullable: false),
                    HorasConcluidas = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "RespostasSimulado",
                columns: table => new
                {
                    RespostaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SimuladoId = table.Column<int>(type: "integer", nullable: false),
                    QuestaoId = table.Column<int>(type: "integer", nullable: false),
                    AlternativaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespostasSimulado", x => x.RespostaId);
                    table.ForeignKey(
                        name: "FK_RespostasSimulado_Alternativas_AlternativaId",
                        column: x => x.AlternativaId,
                        principalTable: "Alternativas",
                        principalColumn: "AlternativaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespostasSimulado_Questoes_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questoes",
                        principalColumn: "QuestaoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespostasSimulado_Simulados_SimuladoId",
                        column: x => x.SimuladoId,
                        principalTable: "Simulados",
                        principalColumn: "SimuladoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SimuladoQuestoes",
                columns: table => new
                {
                    SimuladoQuestaoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SimuladoId = table.Column<int>(type: "integer", nullable: false),
                    QuestaoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimuladoQuestoes", x => x.SimuladoQuestaoId);
                    table.ForeignKey(
                        name: "FK_SimuladoQuestoes_Questoes_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questoes",
                        principalColumn: "QuestaoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SimuladoQuestoes_Simulados_SimuladoId",
                        column: x => x.SimuladoId,
                        principalTable: "Simulados",
                        principalColumn: "SimuladoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alternativas_QuestaoId",
                table: "Alternativas",
                column: "QuestaoId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosEstudo_UsuarioId",
                table: "EventosEstudo",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_HorasLancadas_UsuarioId_Data",
                table: "HorasLancadas",
                columns: new[] { "UsuarioId", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanoMaterias_MateriaId",
                table: "PlanoMaterias",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanoMaterias_PlanoId",
                table: "PlanoMaterias",
                column: "PlanoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosEstudo_UsuarioId",
                table: "PlanosEstudo",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasSimulado_AlternativaId",
                table: "RespostasSimulado",
                column: "AlternativaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasSimulado_QuestaoId",
                table: "RespostasSimulado",
                column: "QuestaoId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasSimulado_SimuladoId",
                table: "RespostasSimulado",
                column: "SimuladoId");

            migrationBuilder.CreateIndex(
                name: "IX_SimuladoQuestoes_QuestaoId",
                table: "SimuladoQuestoes",
                column: "QuestaoId");

            migrationBuilder.CreateIndex(
                name: "IX_SimuladoQuestoes_SimuladoId",
                table: "SimuladoQuestoes",
                column: "SimuladoId");

            migrationBuilder.CreateIndex(
                name: "IX_Simulados_UsuarioId",
                table: "Simulados",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosEstudo");

            migrationBuilder.DropTable(
                name: "HorasLancadas");

            migrationBuilder.DropTable(
                name: "PlanoMaterias");

            migrationBuilder.DropTable(
                name: "RespostasSimulado");

            migrationBuilder.DropTable(
                name: "SimuladoQuestoes");

            migrationBuilder.DropTable(
                name: "Materias");

            migrationBuilder.DropTable(
                name: "PlanosEstudo");

            migrationBuilder.DropTable(
                name: "Alternativas");

            migrationBuilder.DropTable(
                name: "Simulados");

            migrationBuilder.DropTable(
                name: "Questoes");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
