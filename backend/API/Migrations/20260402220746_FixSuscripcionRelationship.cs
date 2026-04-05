using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class FixSuscripcionRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Solo dropear si la columna existe (puede no existir si nunca se insertó una suscripción)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns 
                               WHERE table_name = 'suscripcion' AND column_name = 'NegocioId1') THEN
                        ALTER TABLE suscripcion DROP CONSTRAINT IF EXISTS ""FK_suscripcion_negocio_NegocioId1"";
                        DROP INDEX IF EXISTS ""IX_suscripcion_NegocioId1"";
                        ALTER TABLE suscripcion DROP COLUMN ""NegocioId1"";
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NegocioId1",
                table: "suscripcion",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_suscripcion_NegocioId1",
                table: "suscripcion",
                column: "NegocioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_suscripcion_negocio_NegocioId1",
                table: "suscripcion",
                column: "NegocioId1",
                principalTable: "negocio",
                principalColumn: "Id");
        }
    }
}
