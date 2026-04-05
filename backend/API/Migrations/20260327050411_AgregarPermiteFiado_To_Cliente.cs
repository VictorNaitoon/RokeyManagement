using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPermiteFiado_To_Cliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LimiteCredito",
                table: "cliente",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteFiado",
                table: "cliente",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IdCliente",
                table: "carrito",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_carrito_IdCliente",
                table: "carrito",
                column: "IdCliente");

            migrationBuilder.AddForeignKey(
                name: "FK_carrito_cliente_IdCliente",
                table: "carrito",
                column: "IdCliente",
                principalTable: "cliente",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_carrito_cliente_IdCliente",
                table: "carrito");

            migrationBuilder.DropIndex(
                name: "IX_carrito_IdCliente",
                table: "carrito");

            migrationBuilder.DropColumn(
                name: "LimiteCredito",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "PermiteFiado",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "IdCliente",
                table: "carrito");
        }
    }
}
