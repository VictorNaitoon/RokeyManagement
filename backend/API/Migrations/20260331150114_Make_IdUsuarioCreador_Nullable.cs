using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Make_IdUsuarioCreador_Nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdUsuarioModificador",
                table: "venta",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdUsuarioCreador",
                table: "usuario",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdUsuarioModificador",
                table: "usuario",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdUsuarioModificador",
                table: "producto",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdUsuarioModificador",
                table: "presupuesto",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdUsuarioModificador",
                table: "compra",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdUsuarioModificador",
                table: "caja",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "auditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Entidad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdRegistro = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    Accion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DatosAnteriores = table.Column<string>(type: "text", nullable: true),
                    DatosNuevos = table.Column<string>(type: "text", nullable: true),
                    Id_negocio = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auditoria_negocio_Id_negocio",
                        column: x => x.Id_negocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_auditoria_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_venta_IdUsuarioModificador",
                table: "venta",
                column: "IdUsuarioModificador");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_IdUsuarioCreador",
                table: "usuario",
                column: "IdUsuarioCreador");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_IdUsuarioModificador",
                table: "usuario",
                column: "IdUsuarioModificador");

            migrationBuilder.CreateIndex(
                name: "IX_producto_IdUsuarioModificador",
                table: "producto",
                column: "IdUsuarioModificador");

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_IdUsuarioModificador",
                table: "presupuesto",
                column: "IdUsuarioModificador");

            migrationBuilder.CreateIndex(
                name: "IX_compra_IdUsuarioModificador",
                table: "compra",
                column: "IdUsuarioModificador");

            migrationBuilder.CreateIndex(
                name: "IX_caja_IdUsuarioModificador",
                table: "caja",
                column: "IdUsuarioModificador");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_Id_negocio",
                table: "auditoria",
                column: "Id_negocio");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_IdUsuario",
                table: "auditoria",
                column: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_caja_usuario_IdUsuarioModificador",
                table: "caja",
                column: "IdUsuarioModificador",
                principalTable: "usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_compra_usuario_IdUsuarioModificador",
                table: "compra",
                column: "IdUsuarioModificador",
                principalTable: "usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_presupuesto_usuario_IdUsuarioModificador",
                table: "presupuesto",
                column: "IdUsuarioModificador",
                principalTable: "usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_producto_usuario_IdUsuarioModificador",
                table: "producto",
                column: "IdUsuarioModificador",
                principalTable: "usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_usuario_IdUsuarioCreador",
                table: "usuario",
                column: "IdUsuarioCreador",
                principalTable: "usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_usuario_IdUsuarioModificador",
                table: "usuario",
                column: "IdUsuarioModificador",
                principalTable: "usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_venta_usuario_IdUsuarioModificador",
                table: "venta",
                column: "IdUsuarioModificador",
                principalTable: "usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_caja_usuario_IdUsuarioModificador",
                table: "caja");

            migrationBuilder.DropForeignKey(
                name: "FK_compra_usuario_IdUsuarioModificador",
                table: "compra");

            migrationBuilder.DropForeignKey(
                name: "FK_presupuesto_usuario_IdUsuarioModificador",
                table: "presupuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_producto_usuario_IdUsuarioModificador",
                table: "producto");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_usuario_IdUsuarioCreador",
                table: "usuario");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_usuario_IdUsuarioModificador",
                table: "usuario");

            migrationBuilder.DropForeignKey(
                name: "FK_venta_usuario_IdUsuarioModificador",
                table: "venta");

            migrationBuilder.DropTable(
                name: "auditoria");

            migrationBuilder.DropIndex(
                name: "IX_venta_IdUsuarioModificador",
                table: "venta");

            migrationBuilder.DropIndex(
                name: "IX_usuario_IdUsuarioCreador",
                table: "usuario");

            migrationBuilder.DropIndex(
                name: "IX_usuario_IdUsuarioModificador",
                table: "usuario");

            migrationBuilder.DropIndex(
                name: "IX_producto_IdUsuarioModificador",
                table: "producto");

            migrationBuilder.DropIndex(
                name: "IX_presupuesto_IdUsuarioModificador",
                table: "presupuesto");

            migrationBuilder.DropIndex(
                name: "IX_compra_IdUsuarioModificador",
                table: "compra");

            migrationBuilder.DropIndex(
                name: "IX_caja_IdUsuarioModificador",
                table: "caja");

            migrationBuilder.DropColumn(
                name: "IdUsuarioModificador",
                table: "venta");

            migrationBuilder.DropColumn(
                name: "IdUsuarioCreador",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "IdUsuarioModificador",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "IdUsuarioModificador",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "IdUsuarioModificador",
                table: "presupuesto");

            migrationBuilder.DropColumn(
                name: "IdUsuarioModificador",
                table: "compra");

            migrationBuilder.DropColumn(
                name: "IdUsuarioModificador",
                table: "caja");
        }
    }
}
