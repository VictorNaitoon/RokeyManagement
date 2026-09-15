using API.Data;
using API.DTO.Response.Informes;
using API.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace API.Services.Informes.Formatters;

public class DocxFormatter
{
    private const int TakeCap = 5000;

    public Task<ExportResult> FormatAsync(string tipo, object dto, string periodo, Negocio negocio, CancellationToken ct)
    {
        var filename = $"informe-{tipo}-{periodo}.docx";
        var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        var titulo = GetTitulo(tipo);

        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Title Heading1
            var titlePara = new Paragraph(
                new ParagraphProperties(new ParagraphStyleId() { Val = "Heading1" }),
                new Run(new Text(titulo) { Space = SpaceProcessingModeValues.Preserve })
            );
            body.Append(titlePara);

            // Business header
            body.Append(new Paragraph(new Run(new Text($"{negocio.Nombre} — CUIT {negocio.CUIT}"))));
            if (!string.IsNullOrWhiteSpace(negocio.Direccion))
                body.Append(new Paragraph(new Run(new Text(negocio.Direccion))));
            body.Append(new Paragraph(new Run(new Text($"Periodo: {periodo}"))));
            body.Append(new Paragraph(new Run(new Text($"Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC")) { }));

            // Table
            var table = BuildTable(tipo.ToLowerInvariant(), dto);
            body.Append(table);

            if (IsCapped(tipo.ToLowerInvariant(), dto))
                body.Append(new Paragraph(new Run(new Text("Mostrando primeros 5000 registros.")) { }));

            mainPart.Document.Save();
        }

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

    private static Table BuildTable(string tipo, object dto) => tipo switch
    {
        "ventas-resumen" => BuildVentasResumen((VentasResumenResponse)dto),
        "productos-top" => BuildProductosTop((ProductosTopResponse)dto),
        "flujo-caja" => BuildFlujoCaja((FlujoCajaResponse)dto),
        "ingresos-gastos" => BuildIngresosGastos((IngresosGastosResponse)dto),
        "alertas-stock" => BuildAlertasStock((AlertasStockResponse)dto),
        "ventas-por-pago" => BuildVentasPorPago((VentasPorPagoResponse)dto),
        "ventas-por-vendedor" => BuildVentasPorVendedor((VentasPorVendedorResponse)dto),
        _ => throw new ArgumentException($"Tipo desconocido: {tipo}")
    };

    private static Table CreateTable(params string[] headers)
    {
        var table = new Table(
            new TableProperties(
                new TableGrid(headers.Select(_ => new GridColumn()).ToArray()),
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                )
            )
        );
        var headerRow = new TableRow();
        foreach (var h in headers)
        {
            var cell = new TableCell(new Paragraph(new Run(new Text(h))));
            cell.TableCellProperties = new TableCellProperties(
                new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "D9D9D9" }
            );
            headerRow.Append(cell);
        }
        table.Append(headerRow);
        return table;
    }

    private static TableCell Cell(string text) => new(new Paragraph(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve })));

    private static Table BuildVentasResumen(VentasResumenResponse dto)
    {
        var t = CreateTable("Total Ventas", "Cantidad", "Ticket Promedio", "Anuladas", "Periodo");
        var row = new TableRow();
        row.Append(Cell(FormatCurrency(dto.TotalVentas)));
        row.Append(Cell(dto.CantidadVentas.ToString()));
        row.Append(Cell(FormatCurrency(dto.TicketPromedio)));
        row.Append(Cell(dto.VentasAnuladas.ToString()));
        row.Append(Cell(dto.Periodo));
        t.Append(row);
        return t;
    }

    private static Table BuildProductosTop(ProductosTopResponse dto)
    {
        var t = CreateTable("Producto", "Cantidad", "Monto Total");
        foreach (var p in dto.Productos.Take(TakeCap))
        {
            var row = new TableRow();
            row.Append(Cell(p.Nombre));
            row.Append(Cell(p.CantidadVendida.ToString()));
            row.Append(Cell(FormatCurrency(p.MontoTotal)));
            t.Append(row);
        }
        return t;
    }

    private static Table BuildFlujoCaja(FlujoCajaResponse dto)
    {
        var t = CreateTable("Ingresos", "Egresos", "Balance", "Mov. Ingreso", "Mov. Egreso");
        var row = new TableRow();
        row.Append(Cell(FormatCurrency(dto.Ingresos)));
        row.Append(Cell(FormatCurrency(dto.Egresos)));
        row.Append(Cell(FormatCurrency(dto.Balance)));
        row.Append(Cell(dto.MovimientosIngreso.ToString()));
        row.Append(Cell(dto.MovimientosEgreso.ToString()));
        t.Append(row);
        return t;
    }

    private static Table BuildIngresosGastos(IngresosGastosResponse dto)
    {
        var t = CreateTable("Ventas Totales", "Compras Totales", "Ganancia Bruta", "Margen %");
        var row = new TableRow();
        row.Append(Cell(FormatCurrency(dto.VentasTotales)));
        row.Append(Cell(FormatCurrency(dto.ComprasTotales)));
        row.Append(Cell(FormatCurrency(dto.GananciaBruta)));
        row.Append(Cell($"{dto.MargenPorcentaje:F2}%"));
        t.Append(row);
        return t;
    }

    private static Table BuildAlertasStock(AlertasStockResponse dto)
    {
        var t = CreateTable("Producto", "Actual", "Minimo", "Diferencia");
        foreach (var p in dto.Productos.Take(TakeCap))
        {
            var row = new TableRow();
            row.Append(Cell(p.Nombre));
            row.Append(Cell(p.StockActual.ToString()));
            row.Append(Cell(p.StockMinimo.ToString()));
            row.Append(Cell(p.Diferencia.ToString()));
            t.Append(row);
        }
        return t;
    }

    private static Table BuildVentasPorPago(VentasPorPagoResponse dto)
    {
        var t = CreateTable("Metodo", "Cantidad", "Monto", "%");
        foreach (var m in dto.Metodos.Take(TakeCap))
        {
            var row = new TableRow();
            row.Append(Cell(m.Metodo));
            row.Append(Cell(m.Cantidad.ToString()));
            row.Append(Cell(FormatCurrency(m.Monto)));
            row.Append(Cell($"{m.Porcentaje:F2}%"));
            t.Append(row);
        }
        return t;
    }

    private static Table BuildVentasPorVendedor(VentasPorVendedorResponse dto)
    {
        var t = CreateTable("Vendedor", "Cantidad", "Monto Total");
        foreach (var v in dto.Vendedores.Take(TakeCap))
        {
            var row = new TableRow();
            row.Append(Cell(v.Nombre));
            row.Append(Cell(v.CantidadVentas.ToString()));
            row.Append(Cell(FormatCurrency(v.MontoTotal)));
            t.Append(row);
        }
        return t;
    }

    private static string FormatCurrency(decimal v) =>
        v.ToString("N2", new System.Globalization.CultureInfo("es-AR"));
}
