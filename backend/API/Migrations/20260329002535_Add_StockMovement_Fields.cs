using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Add_StockMovement_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdNegocio",
                table: "movimiento_stock",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Motivo",
                table: "movimiento_stock",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockAnterior",
                table: "movimiento_stock",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockNuevo",
                table: "movimiento_stock",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_stock_IdNegocio",
                table: "movimiento_stock",
                column: "IdNegocio");

            migrationBuilder.AddForeignKey(
                name: "FK_movimiento_stock_negocio_IdNegocio",
                table: "movimiento_stock",
                column: "IdNegocio",
                principalTable: "negocio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_movimiento_stock_negocio_IdNegocio",
                table: "movimiento_stock");

            migrationBuilder.DropIndex(
                name: "IX_movimiento_stock_IdNegocio",
                table: "movimiento_stock");

            migrationBuilder.DropColumn(
                name: "IdNegocio",
                table: "movimiento_stock");

            migrationBuilder.DropColumn(
                name: "Motivo",
                table: "movimiento_stock");

            migrationBuilder.DropColumn(
                name: "StockAnterior",
                table: "movimiento_stock");

            migrationBuilder.DropColumn(
                name: "StockNuevo",
                table: "movimiento_stock");
        }
    }
}
