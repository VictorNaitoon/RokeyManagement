using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class FixSuscripcionNegocioIdColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    has_negocio_id BOOLEAN;
                    has_id_negocio BOOLEAN;
                BEGIN
                    SELECT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'suscripcion' AND column_name = 'NegocioId'
                    ) INTO has_negocio_id;
                    
                    SELECT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'suscripcion' AND column_name = 'IdNegocio'
                    ) INTO has_id_negocio;
                    
                    -- Caso 1: Ambas columnas existen → dropear la vieja
                    IF has_negocio_id AND has_id_negocio THEN
                        ALTER TABLE suscripcion DROP CONSTRAINT IF EXISTS ""FK_suscripcion_negocio_NegocioId"";
                        DROP INDEX IF EXISTS ""IX_suscripcion_NegocioId"";
                        ALTER TABLE suscripcion DROP COLUMN ""NegocioId"";
                        
                    -- Caso 2: Solo existe NegocioId → renombrar
                    ELSIF has_negocio_id AND NOT has_id_negocio THEN
                        ALTER TABLE suscripcion DROP CONSTRAINT IF EXISTS ""FK_suscripcion_negocio_NegocioId"";
                        ALTER TABLE suscripcion RENAME COLUMN ""NegocioId"" TO ""IdNegocio"";
                        ALTER INDEX IF EXISTS ""IX_suscripcion_NegocioId"" RENAME TO ""IX_suscripcion_IdNegocio"";
                    END IF;
                    
                    -- Asegurar FK correcta
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_suscripcion_negocio_IdNegocio') THEN
                        ALTER TABLE suscripcion ADD CONSTRAINT ""FK_suscripcion_negocio_IdNegocio""
                            FOREIGN KEY (""IdNegocio"") REFERENCES negocio(""Id"") ON DELETE CASCADE;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_suscripcion_negocio_IdNegocio",
                table: "suscripcion");

            migrationBuilder.RenameColumn(
                name: "IdNegocio",
                table: "suscripcion",
                newName: "NegocioId");

            migrationBuilder.RenameIndex(
                name: "IX_suscripcion_IdNegocio",
                table: "suscripcion",
                newName: "IX_suscripcion_NegocioId");

            migrationBuilder.AddForeignKey(
                name: "FK_suscripcion_negocio_NegocioId",
                table: "suscripcion",
                column: "NegocioId",
                principalTable: "negocio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
