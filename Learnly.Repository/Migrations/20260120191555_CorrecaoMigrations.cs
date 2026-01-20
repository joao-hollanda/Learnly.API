using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnly.Repository.Migrations
{
    /// <inheritdoc />
    public partial class CorrecaoMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS sp_GerarResumoUsuario;");
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS sp_Listar5PlanosEstudo;");
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS sp_ContarSimuladosUsuario;");
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS sp_ListarUltimos5Simulados;");

        }
    }
}
