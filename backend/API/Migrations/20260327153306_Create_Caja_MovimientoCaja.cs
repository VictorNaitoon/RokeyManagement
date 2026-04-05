using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Create_Caja_MovimientoCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "caja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    IdUsuarioApertura = table.Column<int>(type: "integer", nullable: false),
                    IdUsuarioCierre = table.Column<int>(type: "integer", nullable: true),
                    FechaApertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MontoInicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontoFinal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_caja_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_caja_usuario_IdUsuarioApertura",
                        column: x => x.IdUsuarioApertura,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_caja_usuario_IdUsuarioCierre",
                        column: x => x.IdUsuarioCierre,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimiento_caja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCaja = table.Column<int>(type: "integer", nullable: false),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_caja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_movimiento_caja_caja_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "caja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimiento_caja_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimiento_caja_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_caja_IdNegocio_Estado",
                table: "caja",
                columns: new[] { "IdNegocio", "Estado" },
                unique: true,
                filter: "\"Estado\" = 'Abierta'");

            migrationBuilder.CreateIndex(
                name: "IX_caja_IdUsuarioApertura",
                table: "caja",
                column: "IdUsuarioApertura");

            migrationBuilder.CreateIndex(
                name: "IX_caja_IdUsuarioCierre",
                table: "caja",
                column: "IdUsuarioCierre");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_caja_IdCaja",
                table: "movimiento_caja",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_caja_IdNegocio",
                table: "movimiento_caja",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_caja_IdUsuario",
                table: "movimiento_caja",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movimiento_caja");

            migrationBuilder.DropTable(
                name: "caja");
        }
    }
}
