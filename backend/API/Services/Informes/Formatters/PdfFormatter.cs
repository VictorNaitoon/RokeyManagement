using API.Data;
using API.DTO.Response.Informes;
using API.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace API.Services.Informes.Formatters;

public class PdfFormatter
{
    private const int TakeCap = 5000;

    public Task<ExportResult> FormatAsync(string tipo, object dto, string periodo, Negocio negocio, CancellationToken ct)
    {
        var filename = $"informe-{tipo}-{periodo}.pdf";
        var contentType = "application/pdf";
        var titulo = GetTitulo(tipo);

        QuestPDF.Settings.License = LicenseType.Community;

        using var ms = new MemoryStream();
        Document.Create(container =>
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
                    BuildTable(col, tipo.ToLowerInvariant(), dto);
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
        }).GeneratePdf(ms);

        var bytes = ms.ToArray();
        return Task.FromResult(new ExportResult(bytes, contentType, filename));
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
        _ => false
    };

    private static void BuildTable(ColumnDescriptor col, string tipo, object dto)
    {
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
                case "ingresos-gastos":
                    BuildIngresosGastos(table, (IngresosGastosResponse)dto);
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
        c.Background(Colors.Grey.Lighten3).Padding(4).Text(text).Bold().FontSize(8);

    private static void BodyCell(IContainer c, string text) =>
        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(text).FontSize(8);

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

    private static void BuildIngresosGastos(TableDescriptor table, IngresosGastosResponse dto)
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
