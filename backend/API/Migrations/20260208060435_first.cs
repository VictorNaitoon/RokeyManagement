using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class first : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "negocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CUIT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    LogoURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    IngresosBrutos = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FechaInicioActividades = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PuntoDeVenta = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CondicionVentas = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_negocio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Documento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CondicionIVA = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    FechaAlta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cliente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cliente_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proveedor_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuario_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_categoria_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_categoria_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    IdProveedor = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    FechaCompra = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalGasto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NumeroComprobante = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compra_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_compra_proveedor_IdProveedor",
                        column: x => x.IdProveedor,
                        principalTable: "proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compra_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "presupuesto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    IdCliente = table.Column<int>(type: "integer", nullable: true),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    TotalPresupuesto = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presupuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_presupuesto_cliente_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_presupuesto_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_presupuesto_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "venta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    IdCliente = table.Column<int>(type: "integer", nullable: false),
                    FechaVenta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalVenta = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_venta_cliente_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venta_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_venta_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "producto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    IdUsuarioCreador = table.Column<int>(type: "integer", nullable: false),
                    IdCategoria = table.Column<int>(type: "integer", nullable: true),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CodigoBusqueda = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrecioCompra = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    StockActual = table.Column<int>(type: "integer", nullable: false),
                    StockMinimo = table.Column<int>(type: "integer", nullable: false),
                    ImagenURL = table.Column<string>(type: "text", nullable: true),
                    EsServicio = table.Column<bool>(type: "boolean", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_producto_categoria_IdCategoria",
                        column: x => x.IdCategoria,
                        principalTable: "categoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_producto_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_producto_usuario_IdUsuarioCreador",
                        column: x => x.IdUsuarioCreador,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "factura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdNegocio = table.Column<int>(type: "integer", nullable: false),
                    IdVenta = table.Column<int>(type: "integer", nullable: false),
                    CuitCliente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FechaRealizada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoFactura = table.Column<int>(type: "integer", nullable: false),
                    NumeroComprobante = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CAE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VencimientoCAE = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QR = table.Column<string>(type: "text", nullable: true),
                    CondicionVenta = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_factura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_factura_negocio_IdNegocio",
                        column: x => x.IdNegocio,
                        principalTable: "negocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_factura_venta_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdVenta = table.Column<int>(type: "integer", nullable: false),
                    MetodoPago = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pago_venta_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "carrito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdSesion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioAcumulado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carrito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_carrito_producto_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detalle_compra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdCompra = table.Column<int>(type: "integer", nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_compra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_detalle_compra_compra_IdCompra",
                        column: x => x.IdCompra,
                        principalTable: "compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_compra_producto_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "detalle_presupuesto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdPresupuesto = table.Column<int>(type: "integer", nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioPactado = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_presupuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_detalle_presupuesto_presupuesto_IdPresupuesto",
                        column: x => x.IdPresupuesto,
                        principalTable: "presupuesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_detalle_presupuesto_producto_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "detalle_venta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdVenta = table.Column<int>(type: "integer", nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_venta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_detalle_venta_producto_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_detalle_venta_venta_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movimiento_stock",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdProducto = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    IdVenta = table.Column<int>(type: "integer", nullable: true),
                    IdCompra = table.Column<int>(type: "integer", nullable: true),
                    FechaMovimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    TipoMovimiento = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_stock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_movimiento_stock_compra_IdCompra",
                        column: x => x.IdCompra,
                        principalTable: "compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimiento_stock_producto_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimiento_stock_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimiento_stock_venta_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_carrito_IdProducto",
                table: "carrito",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_categoria_IdNegocio",
                table: "categoria",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_categoria_IdUsuario",
                table: "categoria",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_IdNegocio",
                table: "cliente",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_compra_IdNegocio",
                table: "compra",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_compra_IdProveedor",
                table: "compra",
                column: "IdProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_compra_IdUsuario",
                table: "compra",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_compra_IdCompra",
                table: "detalle_compra",
                column: "IdCompra");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_compra_IdProducto",
                table: "detalle_compra",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_presupuesto_IdPresupuesto",
                table: "detalle_presupuesto",
                column: "IdPresupuesto");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_presupuesto_IdProducto",
                table: "detalle_presupuesto",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_venta_IdProducto",
                table: "detalle_venta",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_venta_IdVenta",
                table: "detalle_venta",
                column: "IdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_factura_IdNegocio",
                table: "factura",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_factura_IdVenta",
                table: "factura",
                column: "IdVenta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_stock_IdCompra",
                table: "movimiento_stock",
                column: "IdCompra");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_stock_IdProducto",
                table: "movimiento_stock",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_stock_IdUsuario",
                table: "movimiento_stock",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_stock_IdVenta",
                table: "movimiento_stock",
                column: "IdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_pago_IdVenta",
                table: "pago",
                column: "IdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_IdCliente",
                table: "presupuesto",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_IdNegocio",
                table: "presupuesto",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_IdUsuario",
                table: "presupuesto",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_producto_IdCategoria",
                table: "producto",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_producto_IdNegocio",
                table: "producto",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_producto_IdUsuarioCreador",
                table: "producto",
                column: "IdUsuarioCreador");

            migrationBuilder.CreateIndex(
                name: "IX_proveedor_IdNegocio",
                table: "proveedor",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_IdNegocio",
                table: "usuario",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_venta_IdCliente",
                table: "venta",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_venta_IdNegocio",
                table: "venta",
                column: "IdNegocio");

            migrationBuilder.CreateIndex(
                name: "IX_venta_IdUsuario",
                table: "venta",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "carrito");

            migrationBuilder.DropTable(
                name: "detalle_compra");

            migrationBuilder.DropTable(
                name: "detalle_presupuesto");

            migrationBuilder.DropTable(
                name: "detalle_venta");

            migrationBuilder.DropTable(
                name: "factura");

            migrationBuilder.DropTable(
                name: "movimiento_stock");

            migrationBuilder.DropTable(
                name: "pago");

            migrationBuilder.DropTable(
                name: "presupuesto");

            migrationBuilder.DropTable(
                name: "compra");

            migrationBuilder.DropTable(
                name: "producto");

            migrationBuilder.DropTable(
                name: "venta");

            migrationBuilder.DropTable(
                name: "proveedor");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropTable(
                name: "cliente");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "negocio");
        }
    }
}
