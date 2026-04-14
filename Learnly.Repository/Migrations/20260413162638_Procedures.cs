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
            // fn_comparar_horas
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_comparar_horas(p_usuario_id INT)
                RETURNS TABLE(horas_hoje INT, horas_ontem INT, diferenca INT)
                LANGUAGE sql STABLE AS $$
                    SELECT
                        COALESCE(SUM(CASE WHEN h.""Data""::date = CURRENT_DATE THEN h.""QuantdadeHoras"" END), 0)::INT,
                        COALESCE(SUM(CASE WHEN h.""Data""::date = CURRENT_DATE - 1 THEN h.""QuantdadeHoras"" END), 0)::INT,
                        COALESCE(SUM(CASE WHEN h.""Data""::date = CURRENT_DATE THEN h.""QuantdadeHoras"" END), 0)::INT -
                        COALESCE(SUM(CASE WHEN h.""Data""::date = CURRENT_DATE - 1 THEN h.""QuantdadeHoras"" END), 0)::INT
                    FROM ""HorasLancadas"" h
                    WHERE h.""UsuarioId"" = p_usuario_id
                    AND h.""Data""::date IN (CURRENT_DATE, CURRENT_DATE - 1);
                $$;
            ");

            // fn_resumo_geral
            migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION fn_resumo_geral(p_usuario_id INT)
            RETURNS TABLE(horas_totais INT, horas_concluidas INT)
            LANGUAGE sql STABLE AS $$
                SELECT
                    COALESCE(SUM(pm.""HorasTotais""), 0)::INT,
                    COALESCE(SUM(pm.""HorasConcluidas""), 0)::INT
                FROM ""PlanosEstudo"" p
                JOIN ""PlanoMaterias"" pm ON pm.""PlanoId"" = p.""PlanoId""
                WHERE p.""UsuarioId"" = p_usuario_id;
            $$;
        ");

            // fn_somar_horas_periodo
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_somar_horas_periodo(p_usuario_id INT, p_inicio DATE, p_fim DATE)
                RETURNS INT AS $$
                SELECT COALESCE(SUM(""QuantdadeHoras""), 0)
                FROM ""HorasLancadas""
                WHERE ""UsuarioId"" = p_usuario_id
                    AND ""Data""::date BETWEEN p_inicio AND p_fim;
                $$ LANGUAGE sql STABLE;");

            // sp_lancar_horas
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE sp_lancar_horas(p_usuario_id INT, p_data DATE, p_quantidade INT)
                LANGUAGE plpgsql AS $$
                BEGIN
                IF EXISTS (SELECT 1 FROM ""HorasLancadas"" WHERE ""UsuarioId"" = p_usuario_id AND ""Data""::date = p_data) THEN
                    UPDATE ""HorasLancadas"" SET ""QuantdadeHoras"" = ""QuantdadeHoras"" + p_quantidade
                    WHERE ""UsuarioId"" = p_usuario_id AND ""Data""::date = p_data;
                ELSE
                    INSERT INTO ""HorasLancadas"" (""UsuarioId"", ""Data"", ""QuantdadeHoras"") VALUES (p_usuario_id, p_data::timestamptz, p_quantidade);
                END IF;
                END;
                $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS fn_comparar_horas(INT);");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS fn_resumo_geral(INT);");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS fn_somar_horas_periodo(INT, DATE, DATE);");
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS sp_lancar_horas(INT, DATE, INT);");
        }
    }
}