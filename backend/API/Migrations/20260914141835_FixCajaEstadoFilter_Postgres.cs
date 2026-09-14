using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class FixCajaEstadoFilter_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_caja_IdNegocio_Estado",
                table: "caja");

            migrationBuilder.CreateIndex(
                name: "IX_caja_IdNegocio_Estado",
                table: "caja",
                columns: new[] { "IdNegocio", "Estado" },
                unique: true,
                filter: "\"Estado\" = 'Abierta'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_caja_IdNegocio_Estado",
                table: "caja");

            migrationBuilder.CreateIndex(
                name: "IX_caja_IdNegocio_Estado",
                table: "caja",
                columns: new[] { "IdNegocio", "Estado" },
                unique: true,
                filter: "[Estado] = 'Abierta'");
        }
    }
}
