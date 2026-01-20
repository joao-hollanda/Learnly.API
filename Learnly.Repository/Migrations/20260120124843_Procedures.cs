using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnly.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Procedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    MateriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cor = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.MateriaId);
                });

            migrationBuilder.CreateTable(
                name: "Questoes",
                columns: table => new
                {
                    QuestaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Disciplina = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ano = table.Column<int>(type: "int", nullable: false),
                    Lingua = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Contexto = table.Column<string>(type: "TEXT", nullable: true),
                    Arquivos = table.Column<string>(type: "TEXT", nullable: true),
                    IntroducaoAlternativa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlternativaCorreta = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questoes", x => x.QuestaoId);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCriacao = table.Column<DateOnly>(type: "date", nullable: false),
                    StatusConta = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alternativas",
                columns: table => new
                {
                    AlternativaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestaoId = table.Column<int>(type: "int", nullable: false),
                    Letra = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Arquivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Correta = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alternativas", x => x.AlternativaId);
                    table.CheckConstraint("CK_Alternativa_TextoOuArquivo", "Texto IS NOT NULL OR Arquivo IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Alternativas_Questoes_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questoes",
                        principalColumn: "QuestaoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanosEstudo",
                columns: table => new
                {
                    PlanoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Objetivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HorasPorSemana = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
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
                    SimuladoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    NotaFinal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Desempenho_QuantidadeDeQuestoes = table.Column<int>(type: "int", nullable: true),
                    Desempenho_QuantidadeDeAcertos = table.Column<int>(type: "int", nullable: true),
                    Desempenho_Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "EventosEstudo",
                columns: table => new
                {
                    EventoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanoId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fim = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosEstudo", x => x.EventoId);
                    table.ForeignKey(
                        name: "FK_EventosEstudo_PlanosEstudo_PlanoId",
                        column: x => x.PlanoId,
                        principalTable: "PlanosEstudo",
                        principalColumn: "PlanoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanoMaterias",
                columns: table => new
                {
                    PlanoMateriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanoId = table.Column<int>(type: "int", nullable: false),
                    MateriaId = table.Column<int>(type: "int", nullable: false),
                    HorasTotais = table.Column<int>(type: "int", nullable: false),
                    HorasConcluidas = table.Column<int>(type: "int", nullable: false)
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
                    RespostaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SimuladoId = table.Column<int>(type: "int", nullable: false),
                    QuestaoId = table.Column<int>(type: "int", nullable: false),
                    AlternativaId = table.Column<int>(type: "int", nullable: false)
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
                    SimuladoQuestaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SimuladoId = table.Column<int>(type: "int", nullable: false),
                    QuestaoId = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_EventosEstudo_PlanoId",
                table: "EventosEstudo",
                column: "PlanoId");

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

            migrationBuilder.Sql(@"
-------------------------------------------------
-- LISTAR OS 5 ÚLTIMOS SIMULADOS DO USUÁRIO
-------------------------------------------------
CREATE OR ALTER PROCEDURE sp_ListarUltimos5Simulados
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 5
        s.SimuladoId,
        s.UsuarioId,
        s.Data,
        s.NotaFinal
    FROM Simulados s
    WHERE s.UsuarioId = @UsuarioId
    ORDER BY s.Data DESC;
END;
");

            migrationBuilder.Sql(@"
-------------------------------------------------
-- CONTAR TOTAL DE SIMULADOS DO USUÁRIO
-------------------------------------------------
CREATE OR ALTER PROCEDURE sp_ContarSimuladosUsuario
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) AS Total
    FROM Simulados
    WHERE UsuarioId = @UsuarioId;
END;
");

            migrationBuilder.Sql(@"
-------------------------------------------------
-- LISTAR OS 5 ÚLTIMOS PLANOS DE ESTUDO DO USUÁRIO
-------------------------------------------------
CREATE OR ALTER PROCEDURE sp_Listar5PlanosEstudo
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 5
        p.PlanoId,
        p.UsuarioId,
        p.Ativo,
        p.DataInicio,
        p.DataFim
    FROM PlanosEstudo p
    WHERE p.UsuarioId = @UsuarioId
    ORDER BY p.Ativo DESC, p.PlanoId DESC;
END;
");

            migrationBuilder.Sql(@"
-------------------------------------------------
-- GERAR RESUMO GERAL (HORAS TOTAIS E CONCLUÍDAS)
-------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GerarResumoGeralUsuario
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ISNULL(SUM(pm.HorasTotais), 0)     AS HorasTotais,
        ISNULL(SUM(pm.HorasConcluidas), 0) AS HorasConcluidas
    FROM PlanosEstudo p
    INNER JOIN PlanoMaterias pm ON pm.PlanoId = p.PlanoId
    WHERE p.UsuarioId = @UsuarioId;
END;
");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosEstudo");

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

            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS sp_GerarResumoUsuario;");
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS sp_Listar5PlanosEstudo;");
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS sp_ContarSimuladosUsuario;");
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS sp_ListarUltimos5Simulados;");


        }
    }
}
