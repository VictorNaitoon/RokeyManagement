using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Add_Suscripciones_To_Negocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NegocioId",
                table: "suscripcion",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_suscripcion_NegocioId",
                table: "suscripcion",
                column: "NegocioId");

            migrationBuilder.AddForeignKey(
                name: "FK_suscripcion_negocio_NegocioId",
                table: "suscripcion",
                column: "NegocioId",
                principalTable: "negocio",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_suscripcion_negocio_NegocioId",
                table: "suscripcion");

            migrationBuilder.DropIndex(
                name: "IX_suscripcion_NegocioId",
                table: "suscripcion");

            migrationBuilder.DropColumn(
                name: "NegocioId",
                table: "suscripcion");
        }
    }
}
