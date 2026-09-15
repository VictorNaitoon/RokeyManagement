using API.DTO.Response.Informes;
using API.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace API.Services.Informes.Formatters;

public class PdfFormatter
{
    private const int TakeCap = 5000;
    private readonly ILogger<PdfFormatter> _logger;

    public PdfFormatter(ILogger<PdfFormatter> logger)
    {
        _logger = logger;
    }

    // Parameterless fallback for tests where DI may not provide logger (not used in production)
    public PdfFormatter() : this(LoggerFactory.Create(b => { }).CreateLogger<PdfFormatter>()) { }

    public Task<ExportResult> FormatAsync(string tipo, object dto, string periodo, Negocio negocio, CancellationToken ct)
    {
        var filename = $"informe-{tipo}-{periodo}.pdf";
        var contentType = "application/pdf";
        var titulo = GetTitulo(tipo);

        // License is set globally in Program.cs; keep idempotent fallback for safety
        // QuestPDF 2024+ throws InvalidOperationException if license not set
        QuestPDF.Settings.License = LicenseType.Community;

        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().Text($"{negocio.Nombre} — CUIT {negocio.CUIT}").FontSize(14).Bold();
                        if (!string.IsNullOrWhiteSpace(negocio.Direccion))
                            col.Item().Text(negocio.Direccion).FontSize(9).FontColor(Colors.Grey.Medium);
                        col.Item().Text($"Informe: {titulo} — Periodo: {periodo}").FontSize(9);
                        col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        BuildContent(col, tipo.ToLowerInvariant(), dto);
                        var isCapped = IsCapped(tipo.ToLowerInvariant(), dto);
                        if (isCapped)
                            col.Item().PaddingTop(6).Text("Mostrando primeros 5000 registros.").FontSize(7).Italic().FontColor(Colors.Grey.Medium);
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text($"Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(7).FontColor(Colors.Grey.Medium);
                        row.ConstantItem(80).AlignRight().Text(t =>
                        {
                            t.Span("Pagina ");
                            t.CurrentPageNumber();
                            t.Span(" / ");
                            t.TotalPages();
                        });
                    });
                });
            });

            // Use GeneratePdf() returning byte[] — avoids MemoryStream lifecycle issues and ObjectDisposedException
            var bytes = document.GeneratePdf();
            return Task.FromResult(new ExportResult(bytes, contentType, filename));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF generation failed tipo={Tipo} periodo={Periodo}", tipo, periodo);
            // Fallback: never crash the host — return a valid PDF with the error message
            try
            {
                var errorDoc = Document.Create(c2 =>
                {
                    c2.Page(p =>
                    {
                        p.Size(PageSizes.A4);
                        p.Margin(20);
                        p.DefaultTextStyle(x => x.FontSize(10));
                        p.Content().Column(col =>
                        {
                            col.Item().Text($"Error al generar informe: {titulo}").Bold().FontSize(14);
                            col.Item().PaddingTop(8).Text($"Periodo: {periodo}").FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().PaddingTop(8).Text("No se pudo generar el contenido solicitado. Intente nuevamente o contacte soporte.").FontSize(9);
                            col.Item().PaddingTop(4).Text($"Detalle tecnico: {ex.Message}").FontSize(7).FontColor(Colors.Grey.Medium);
                        });
                    });
                });
                var errorBytes = errorDoc.GeneratePdf();
                return Task.FromResult(new ExportResult(errorBytes, contentType, filename));
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "PDF fallback generation also failed");
                var fallbackBytes = System.Text.Encoding.UTF8.GetBytes($"Error generando PDF: {ex.Message}");
                return Task.FromResult(new ExportResult(fallbackBytes, contentType, filename));
            }
        }
    }

    private static string GetTitulo(string tipo) => tipo.ToLowerInvariant() switch
    {
        "ventas-resumen" => "Resumen de Ventas",
        "productos-top" => "Productos Mas Vendidos",
        "flujo-caja" => "Flujo de Caja",
        "ingresos-gastos" => "Ingresos y Gastos",
        "alertas-stock" => "Alertas de Stock",
        "ventas-por-pago" => "Ventas por Metodo de Pago",
        "ventas-por-vendedor" => "Ventas por Vendedor",
        _ => tipo
    };

    private static bool IsCapped(string tipo, object dto) => tipo switch
    {
        "productos-top" => ((ProductosTopResponse)dto).Productos.Count >= TakeCap,
        "alertas-stock" => ((AlertasStockResponse)dto).Productos.Count >= TakeCap,
        "ventas-por-pago" => ((VentasPorPagoResponse)dto).Metodos.Count >= TakeCap,
        "ventas-por-vendedor" => ((VentasPorVendedorResponse)dto).Vendedores.Count >= TakeCap,
        "ingresos-gastos" => (((IngresosGastosResponse)dto).DetalleVentas?.Count ?? 0) >= TakeCap
                           || (((IngresosGastosResponse)dto).DetalleCompras?.Count ?? 0) >= TakeCap,
        _ => false
    };

    private static void BuildContent(ColumnDescriptor col, string tipo, object dto)
    {
        // ingresos-gastos needs multiple tables (summary + detalle ventas + detalle compras)
        if (tipo == "ingresos-gastos")
        {
            BuildIngresosGastosContent(col, (IngresosGastosResponse)dto);
            return;
        }

        col.Item().Table(table =>
        {
            switch (tipo)
            {
                case "ventas-resumen":
                    BuildVentasResumen(table, (VentasResumenResponse)dto);
                    break;
                case "productos-top":
                    BuildProductosTop(table, (ProductosTopResponse)dto);
                    break;
                case "flujo-caja":
                    BuildFlujoCaja(table, (FlujoCajaResponse)dto);
                    break;
                case "alertas-stock":
                    BuildAlertasStock(table, (AlertasStockResponse)dto);
                    break;
                case "ventas-por-pago":
                    BuildVentasPorPago(table, (VentasPorPagoResponse)dto);
                    break;
                case "ventas-por-vendedor":
                    BuildVentasPorVendedor(table, (VentasPorVendedorResponse)dto);
                    break;
            }
        });
    }

    private static void HeaderCell(IContainer c, string text) =>
        c.Background(Colors.Grey.Lighten3).Padding(4).Text(text ?? string.Empty).Bold().FontSize(8);

    private static void BodyCell(IContainer c, string text) =>
        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(text ?? string.Empty).FontSize(8);

    private static void TotalCell(IContainer c, string text) =>
        c.Background(Colors.Grey.Lighten2).Padding(4).Text(text ?? string.Empty).Bold().FontSize(8);

    private static void BuildVentasResumen(TableDescriptor table, VentasResumenResponse dto)
    {
        table.ColumnsDefinition(c =>
        {
            c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
        });
        table.Header(h =>
        {
            HeaderCell(h.Cell(), "Total Ventas");
            HeaderCell(h.Cell(), "Cantidad");
            HeaderCell(h.Cell(), "Ticket Promedio");
            HeaderCell(h.Cell(), "Anuladas");
            HeaderCell(h.Cell(), "Periodo");
        });
        BodyCell(table.Cell(), FormatCurrency(dto.TotalVentas));
        BodyCell(table.Cell(), dto.CantidadVentas.ToString());
        BodyCell(table.Cell(), FormatCurrency(dto.TicketPromedio));
        BodyCell(table.Cell(), dto.VentasAnuladas.ToString());
        BodyCell(table.Cell(), dto.Periodo);
    }

    private static void BuildProductosTop(TableDescriptor table, ProductosTopResponse dto)
    {
        table.ColumnsDefinition(c => { c.RelativeColumn(3); c.ConstantColumn(60); c.ConstantColumn(90); });
        table.Header(h => { HeaderCell(h.Cell(), "Producto"); HeaderCell(h.Cell(), "Cantidad"); HeaderCell(h.Cell(), "Monto Total"); });
        foreach (var p in dto.Productos.Take(TakeCap))
        {
            BodyCell(table.Cell(), p.Nombre);
            BodyCell(table.Cell(), p.CantidadVendida.ToString());
            BodyCell(table.Cell(), FormatCurrency(p.MontoTotal));
        }
    }

    private static void BuildFlujoCaja(TableDescriptor table, FlujoCajaResponse dto)
    {
        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
        table.Header(h =>
        {
            HeaderCell(h.Cell(), "Ingresos"); HeaderCell(h.Cell(), "Egresos"); HeaderCell(h.Cell(), "Balance");
            HeaderCell(h.Cell(), "Mov. Ingreso"); HeaderCell(h.Cell(), "Mov. Egreso");
        });
        BodyCell(table.Cell(), FormatCurrency(dto.Ingresos));
        BodyCell(table.Cell(), FormatCurrency(dto.Egresos));
        BodyCell(table.Cell(), FormatCurrency(dto.Balance));
        BodyCell(table.Cell(), dto.MovimientosIngreso.ToString());
        BodyCell(table.Cell(), dto.MovimientosEgreso.ToString());
    }

    private static void BuildIngresosGastosContent(ColumnDescriptor col, IngresosGastosResponse dto)
    {
        // Summary table (GananciaBruta / Margen untouched)
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
            table.Header(h =>
            {
                HeaderCell(h.Cell(), "Ventas Totales"); HeaderCell(h.Cell(), "Compras Totales");
                HeaderCell(h.Cell(), "Ganancia Bruta"); HeaderCell(h.Cell(), "Margen %");
            });
            BodyCell(table.Cell(), FormatCurrency(dto.VentasTotales));
            BodyCell(table.Cell(), FormatCurrency(dto.ComprasTotales));
            BodyCell(table.Cell(), FormatCurrency(dto.GananciaBruta));
            BodyCell(table.Cell(), $"{dto.MargenPorcentaje:F2}%");
        });

        // Detail Ventas
        var detalleVentas = dto.DetalleVentas ?? new List<DetalleVentaInforme>();
        col.Item().PaddingTop(10).Text("Detalle de Ventas").Bold().FontSize(10);
        if (detalleVentas.Count == 0)
        {
            col.Item().PaddingTop(2).Text("Sin movimientos de venta en el periodo.").FontSize(8).Italic().FontColor(Colors.Grey.Medium);
        }
        else
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(3); c.ConstantColumn(55); c.ConstantColumn(75); c.ConstantColumn(85); });
                table.Header(h =>
                {
                    HeaderCell(h.Cell(), "Producto");
                    HeaderCell(h.Cell(), "Cant.");
                    HeaderCell(h.Cell(), "P. Unit.");
                    HeaderCell(h.Cell(), "Subtotal");
                });
                foreach (var d in detalleVentas.Take(TakeCap))
                {
                    BodyCell(table.Cell(), d.Producto);
                    BodyCell(table.Cell(), d.Cantidad.ToString());
                    BodyCell(table.Cell(), FormatCurrency(d.PrecioUnitario));
                    BodyCell(table.Cell(), FormatCurrency(d.Subtotal));
                }
                // Total row
                TotalCell(table.Cell(), "TOTAL");
                TotalCell(table.Cell(), detalleVentas.Sum(x => x.Cantidad).ToString());
                TotalCell(table.Cell(), "");
                TotalCell(table.Cell(), FormatCurrency(dto.VentasTotales));
            });
        }

        // Detail Compras
        var detalleCompras = dto.DetalleCompras ?? new List<DetalleCompraInforme>();
        col.Item().PaddingTop(10).Text("Detalle de Compras").Bold().FontSize(10);
        if (detalleCompras.Count == 0)
        {
            col.Item().PaddingTop(2).Text("Sin movimientos de compra en el periodo.").FontSize(8).Italic().FontColor(Colors.Grey.Medium);
        }
        else
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(3); c.ConstantColumn(55); c.ConstantColumn(75); c.ConstantColumn(85); });
                table.Header(h =>
                {
                    HeaderCell(h.Cell(), "Producto");
                    HeaderCell(h.Cell(), "Cant.");
                    HeaderCell(h.Cell(), "Costo Unit.");
                    HeaderCell(h.Cell(), "Subtotal");
                });
                foreach (var d in detalleCompras.Take(TakeCap))
                {
                    BodyCell(table.Cell(), d.Producto);
                    BodyCell(table.Cell(), d.Cantidad.ToString());
                    BodyCell(table.Cell(), FormatCurrency(d.CostoUnitario));
                    BodyCell(table.Cell(), FormatCurrency(d.Subtotal));
                }
                TotalCell(table.Cell(), "TOTAL");
                TotalCell(table.Cell(), detalleCompras.Sum(x => x.Cantidad).ToString());
                TotalCell(table.Cell(), "");
                TotalCell(table.Cell(), FormatCurrency(dto.ComprasTotales));
            });
        }
    }

    private static void BuildAlertasStock(TableDescriptor table, AlertasStockResponse dto)
    {
        table.ColumnsDefinition(c => { c.RelativeColumn(3); c.ConstantColumn(60); c.ConstantColumn(60); c.ConstantColumn(60); });
        table.Header(h => { HeaderCell(h.Cell(), "Producto"); HeaderCell(h.Cell(), "Actual"); HeaderCell(h.Cell(), "Minimo"); HeaderCell(h.Cell(), "Diferencia"); });
        foreach (var p in dto.Productos.Take(TakeCap))
        {
            BodyCell(table.Cell(), p.Nombre);
            BodyCell(table.Cell(), p.StockActual.ToString());
            BodyCell(table.Cell(), p.StockMinimo.ToString());
            BodyCell(table.Cell(), p.Diferencia.ToString());
        }
    }

    private static void BuildVentasPorPago(TableDescriptor table, VentasPorPagoResponse dto)
    {
        table.ColumnsDefinition(c => { c.RelativeColumn(2); c.ConstantColumn(60); c.ConstantColumn(90); c.ConstantColumn(60); });
        table.Header(h => { HeaderCell(h.Cell(), "Metodo"); HeaderCell(h.Cell(), "Cantidad"); HeaderCell(h.Cell(), "Monto"); HeaderCell(h.Cell(), "%"); });
        foreach (var m in dto.Metodos.Take(TakeCap))
        {
            BodyCell(table.Cell(), m.Metodo);
            BodyCell(table.Cell(), m.Cantidad.ToString());
            BodyCell(table.Cell(), FormatCurrency(m.Monto));
            BodyCell(table.Cell(), $"{m.Porcentaje:F2}%");
        }
    }

    private static void BuildVentasPorVendedor(TableDescriptor table, VentasPorVendedorResponse dto)
    {
        table.ColumnsDefinition(c => { c.RelativeColumn(3); c.ConstantColumn(60); c.ConstantColumn(90); });
        table.Header(h => { HeaderCell(h.Cell(), "Vendedor"); HeaderCell(h.Cell(), "Cantidad"); HeaderCell(h.Cell(), "Monto Total"); });
        foreach (var v in dto.Vendedores.Take(TakeCap))
        {
            BodyCell(table.Cell(), v.Nombre);
            BodyCell(table.Cell(), v.CantidadVentas.ToString());
            BodyCell(table.Cell(), FormatCurrency(v.MontoTotal));
        }
    }

    private static string FormatCurrency(decimal v) =>
        v.ToString("N2", new System.Globalization.CultureInfo("es-AR"));
}
