using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class SaaS_Initial_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "metrica_uso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Anio = table.Column<int>(type: "integer", nullable: false),
                    TotalUsuarios = table.Column<int>(type: "integer", nullable: false),
                    TotalProductos = table.Column<int>(type: "integer", nullable: false),
                    TotalTransacciones = table.Column<int>(type: "integer", nullable: false),
                    AlmacenamientoBytes = table.Column<long>(type: "bigint", nullable: false),
                    TotalAPICalls = table.Column<int>(type: "integer", nullable: false),
                    UltimaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metrica_uso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_metrica_uso_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrecioMensual = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PrecioAnual = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxUsuarios = table.Column<int>(type: "integer", nullable: false),
                    MaxProductos = table.Column<int>(type: "integer", nullable: false),
                    MaxTransaccionesMes = table.Column<int>(type: "integer", nullable: false),
                    SoportePrioritario = table.Column<bool>(type: "boolean", nullable: false),
                    MultiSucursal = table.Column<bool>(type: "boolean", nullable: false),
                    APIAccess = table.Column<bool>(type: "boolean", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "super_admin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    UltimoLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_super_admin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "suscripcion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    IdPlan = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaProximoPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    TipoFacturacion = table.Column<int>(type: "integer", nullable: false),
                    FechaCancelacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suscripcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_suscripcion_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_suscripcion_plan_IdPlan",
                        column: x => x.IdPlan,
                        principalTable: "plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pago_suscripcion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdSuscripcion = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Metodo = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Detalles = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pago_suscripcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pago_suscripcion_suscripcion_IdSuscripcion",
                        column: x => x.IdSuscripcion,
                        principalTable: "suscripcion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_metrica_uso_IdNegocio",
                table: "metrica_uso",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_pago_suscripcion_IdSuscripcion",
                table: "pago_suscripcion",
                column: "IdSuscripcion");

            migrationBuilder.CreateIndex(
                name: "IX_suscripcion_IdNegocio",
                table: "suscripcion",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_suscripcion_IdPlan",
                table: "suscripcion",
                column: "IdPlan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "metrica_uso");

            migrationBuilder.DropTable(
                name: "pago_suscripcion");

            migrationBuilder.DropTable(
                name: "super_admin");

            migrationBuilder.DropTable(
                name: "suscripcion");

            migrationBuilder.DropTable(
                name: "plan");
        }
    }
}
