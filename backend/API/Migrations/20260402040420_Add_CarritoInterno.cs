using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Add_CarritoInterno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LimiteCarritosActivos",
                table: "negocio",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "carrito_interno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    NegocioId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carrito_interno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_carrito_interno_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_carrito_interno_negocio_NegocioId",
                        column: x => x.NegocioId,
                        principalTable: "negocio",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_carrito_interno_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "carrito_interno_item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarritoInternoId = table.Column<int>(type: "integer", nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carrito_interno_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_carrito_interno_item_carrito_interno_CarritoInternoId",
                        column: x => x.CarritoInternoId,
                        principalTable: "carrito_interno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_carrito_interno_item_producto_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_carrito_interno_Estado",
                table: "carrito_interno",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_carrito_interno_IdNegocio",
                table: "carrito_interno",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_carrito_interno_IdUsuario",
                table: "carrito_interno",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_carrito_interno_NegocioId",
                table: "carrito_interno",
                column: "NegocioId");

            migrationBuilder.CreateIndex(
                name: "IX_carrito_interno_item_CarritoInternoId",
                table: "carrito_interno_item",
                column: "CarritoInternoId");

            migrationBuilder.CreateIndex(
                name: "IX_carrito_interno_item_IdProducto",
                table: "carrito_interno_item",
                column: "IdProducto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "carrito_interno_item");

            migrationBuilder.DropTable(
                name: "carrito_interno");

            migrationBuilder.DropColumn(
                name: "LimiteCarritosActivos",
                table: "negocio");
        }
    }
}
