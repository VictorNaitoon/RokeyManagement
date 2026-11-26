using API.Data;
using API.DTO.Response.Ventas;
using API.Models;
using API.Services.Common;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace API.Services.Ventas
{
    /// <summary>
    /// Genera el comprobante de una venta en PDF o PNG para imprimir, descargar o compartir.
    /// Reutiliza el mismo estilo de ticket que muestra el panel (80 mm de ancho).
    /// </summary>
    public class ComprobanteVentaService : IComprobanteVentaService
    {
        private const string FormatoPdf = "pdf";
        private const string FormatoPng = "png";

        private readonly IVentaService _ventaService;
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public ComprobanteVentaService(
            IVentaService ventaService,
            AppDbContext context,
            ICurrentUserService currentUser)
        {
            _ventaService = ventaService;
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<ComprobanteVentaFile?> GenerarAsync(int ventaId, string formato, CancellationToken ct)
        {
            var formatoNormalizado = (formato ?? string.Empty).Trim().ToLowerInvariant();
            if (formatoNormalizado is not (FormatoPdf or FormatoPng))
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("formato", "formato debe ser pdf o png.")
                });
            }

            // ObtenerVentaPorIdAsync ya filtra por Id_negocio: no se puede generar el
            // comprobante de una venta de otro negocio.
            var venta = await _ventaService.ObtenerVentaPorIdAsync(ventaId, ct);
            if (venta == null)
            {
                return null;
            }

            var negocio = await _context.Negocios
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == _currentUser.NegocioId, ct)
                ?? new Negocio { Nombre = "Mi Local" };

            // Program.cs ya configura la licencia; se repite por si el servicio se usa aislado.
            QuestPDF.Settings.License = LicenseType.Community;

            var document = CrearDocumento(venta, negocio);

            if (formatoNormalizado == FormatoPdf)
            {
                return new ComprobanteVentaFile(
                    document.GeneratePdf(),
                    "application/pdf",
                    $"venta-{venta.Id}.pdf");
            }

            var imagen = document
                .GenerateImages(new ImageGenerationSettings { RasterDpi = 300 })
                .First();

            return new ComprobanteVentaFile(imagen, "image/png", $"venta-{venta.Id}.png");
        }

        private static Document CrearDocumento(VentaResponse venta, Negocio negocio)
        {
            // Alto dinámico: el ticket entra en una sola página para que la imagen salga completa.
            var altoMm = Math.Clamp(100 + venta.Detalles.Count * 8 + venta.Pagos.Count * 6, 130, 900);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(80, altoMm, Unit.Millimetre);
                    page.Margin(5, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Lato).FontColor(Colors.Black));

                    page.Content().Column(col =>
                    {
                        col.Spacing(2);

                        col.Item().AlignCenter().Text(negocio.Nombre).Bold().FontSize(12);

                        if (!string.IsNullOrWhiteSpace(negocio.Direccion))
                        {
                            col.Item().AlignCenter().Text(negocio.Direccion);
                        }

                        if (!string.IsNullOrWhiteSpace(negocio.Telefono))
                        {
                            col.Item().AlignCenter().Text($"Tel: {negocio.Telefono}");
                        }

                        col.Item().PaddingVertical(4).LineHorizontal(0.5f);

                        col.Item().AlignCenter().Text(text =>
                        {
                            text.Span("Venta N°: ").Bold();
                            text.Span(venta.Id.ToString());
                        });

                        col.Item().AlignCenter().Text($"Fecha: {FormatearFecha(venta.Fecha)}");

                        col.Item().Text(text =>
                        {
                            text.Span("Vendedor: ").Bold();
                            text.Span(string.IsNullOrWhiteSpace(venta.NombreUsuario) ? "-" : venta.NombreUsuario);
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("Estado: ").Bold();
                            text.Span(venta.Estado == "Activa" ? "Pagado" : "Anulada");
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("Cliente: ").Bold();
                            text.Span(string.IsNullOrWhiteSpace(venta.NombreCliente) ? "-" : venta.NombreCliente);
                        });

                        col.Item().PaddingVertical(4).LineHorizontal(0.5f);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(32);
                                columns.RelativeColumn();
                                columns.ConstantColumn(48);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Cant.");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Producto");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Valor");
                            });

                            foreach (var detalle in venta.Detalles)
                            {
                                table.Cell().Element(BodyCell).AlignCenter().Text(detalle.Cantidad.ToString());
                                table.Cell().Element(BodyCell).AlignCenter().Text(
                                    string.IsNullOrWhiteSpace(detalle.NombreProducto)
                                        ? "Producto eliminado"
                                        : detalle.NombreProducto);
                                table.Cell().Element(BodyCell).AlignCenter().Text($"${detalle.PrecioUnitario:F2}");
                            }
                        });

                        col.Item().PaddingVertical(4).LineHorizontal(0.5f);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Total:").Bold().FontSize(11);
                            row.RelativeItem().AlignRight().Text($"${venta.TotalVenta:F2}").Bold().FontSize(11);
                        });

                        col.Item().PaddingVertical(4).LineHorizontal(0.5f);

                        foreach (var pago in venta.Pagos)
                        {
                            col.Item().AlignCenter().Text(text =>
                            {
                                text.Span("Método de pago: ").Bold();
                                text.Span(NombreMetodoPago(pago.MetodoPago));
                            });
                        }

                        col.Item().PaddingVertical(4).LineHorizontal(0.5f);

                        col.Item().AlignCenter().Text("¡Muchas gracias por su compra!").Bold().FontSize(11);
                    });
                });
            });
        }

        private static IContainer HeaderCell(IContainer container) =>
            container.BorderBottom(0.5f).BorderColor(Colors.Black).PaddingVertical(2);

        private static IContainer BodyCell(IContainer container) =>
            container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2);

        /// <summary>
        /// El comprobante se muestra en hora de Argentina (UTC-3, sin horario de verano),
        /// igual que lo ve el usuario en el navegador.
        /// </summary>
        private static string FormatearFecha(DateTime fecha) =>
            DateTime.SpecifyKind(fecha, DateTimeKind.Utc).AddHours(-3).ToString("dd/MM/yyyy HH:mm");

        private static string NombreMetodoPago(Enums.MetodoPago metodo) => metodo switch
        {
            Enums.MetodoPago.Efectivo => "Efectivo",
            Enums.MetodoPago.TarjetaCredito => "Tarjeta de Crédito",
            Enums.MetodoPago.TarjetaDebito => "Tarjeta de Débito",
            Enums.MetodoPago.TransferenciaBancaria => "Transferencia",
            Enums.MetodoPago.MercadoPago => "Mercado Pago",
            _ => metodo.ToString()
        };
    }
}
