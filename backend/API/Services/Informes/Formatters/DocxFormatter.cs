using API.DTO.Response.Informes;
using API.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace API.Services.Informes.Formatters;

public class DocxFormatter
{
    private const int TakeCap = 5000;
    private readonly ILogger<DocxFormatter> _logger;

    public DocxFormatter(ILogger<DocxFormatter> logger)
    {
        _logger = logger;
    }

    // Fallback for tests / manual instantiation
    public DocxFormatter() : this(LoggerFactory.Create(b => { }).CreateLogger<DocxFormatter>()) { }

    public Task<ExportResult> FormatAsync(string tipo, object dto, string periodo, Negocio negocio, CancellationToken ct)
    {
        var filename = $"informe-{tipo}-{periodo}.docx";
        var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        var titulo = GetTitulo(tipo);

        try
        {
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

                // Content: ingresos-gastos needs multiple tables
                if (tipo.ToLowerInvariant() == "ingresos-gastos")
                {
                    AppendIngresosGastosContent(body, (IngresosGastosResponse)dto);
                }
                else
                {
                    var table = BuildTable(tipo.ToLowerInvariant(), dto);
                    body.Append(table);
                }

                if (IsCapped(tipo.ToLowerInvariant(), dto))
                    body.Append(new Paragraph(new Run(new Text("Mostrando primeros 5000 registros.")) { }));

                mainPart.Document.Save();
            }

            var bytes = ms.ToArray();
            return Task.FromResult(new ExportResult(bytes, contentType, filename));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DOCX generation failed tipo={Tipo} periodo={Periodo}", tipo, periodo);
            // Fallback: never crash the host — return a valid DOCX with the error message
            try
            {
                using var ms = new MemoryStream();
                using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
                {
                    var mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    var body = mainPart.Document.AppendChild(new Body());
                    body.Append(new Paragraph(new Run(new Text($"Error al generar informe: {titulo}"))));
                    body.Append(new Paragraph(new Run(new Text($"Periodo: {periodo}"))));
                    body.Append(new Paragraph(new Run(new Text($"Detalle tecnico: {ex.Message}"))));
                    body.Append(new Paragraph(new Run(new Text("No se pudo generar el contenido solicitado. Intente nuevamente o contacte soporte."))));
                    mainPart.Document.Save();
                }
                var errorBytes = ms.ToArray();
                return Task.FromResult(new ExportResult(errorBytes, contentType, filename));
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "DOCX fallback generation also failed");
                var fallbackBytes = System.Text.Encoding.UTF8.GetBytes($"Error generando DOCX: {ex.Message}");
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

    private static Table BuildTable(string tipo, object dto) => tipo switch
    {
        "ventas-resumen" => BuildVentasResumen((VentasResumenResponse)dto),
        "productos-top" => BuildProductosTop((ProductosTopResponse)dto),
        "flujo-caja" => BuildFlujoCaja((FlujoCajaResponse)dto),
        "ingresos-gastos" => BuildIngresosGastosSummary((IngresosGastosResponse)dto),
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

    private static TableCell Cell(string text) => new(new Paragraph(new Run(new Text(text ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve })));

    private static void AppendIngresosGastosContent(Body body, IngresosGastosResponse dto)
    {
        // Summary (GananciaBruta / Margen untouched)
        body.Append(BuildIngresosGastosSummary(dto));

        // Detalle Ventas
        var detalleVentas = dto.DetalleVentas ?? new List<DetalleVentaInforme>();
        body.Append(new Paragraph(
            new ParagraphProperties(new ParagraphStyleId() { Val = "Heading2" }),
            new Run(new Text("Detalle de Ventas") { Space = SpaceProcessingModeValues.Preserve })
        ));
        if (detalleVentas.Count == 0)
        {
            body.Append(new Paragraph(new Run(new Text("Sin movimientos de venta en el periodo."))));
        }
        else
        {
            var tVentas = CreateTable("Producto", "Cant.", "P. Unit.", "Subtotal");
            foreach (var d in detalleVentas.Take(TakeCap))
            {
                var row = new TableRow();
                row.Append(Cell(d.Producto));
                row.Append(Cell(d.Cantidad.ToString()));
                row.Append(Cell(FormatCurrency(d.PrecioUnitario)));
                row.Append(Cell(FormatCurrency(d.Subtotal)));
                tVentas.Append(row);
            }
            // Total row with shading
            var totalRowV = new TableRow();
            var totalCellLabel = Cell("TOTAL");
            totalCellLabel.TableCellProperties = new TableCellProperties(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "E8E8E8" });
            totalRowV.Append(totalCellLabel);
            var totalCantCell = Cell(detalleVentas.Sum(x => x.Cantidad).ToString());
            totalCantCell.TableCellProperties = new TableCellProperties(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "E8E8E8" });
            totalRowV.Append(totalCantCell);
            var emptyCell = Cell("");
            emptyCell.TableCellProperties = new TableCellProperties(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "E8E8E8" });
            totalRowV.Append(emptyCell);
            var totalMontoCell = Cell(FormatCurrency(dto.VentasTotales));
            totalMontoCell.TableCellProperties = new TableCellProperties(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "E8E8E8" });
            totalRowV.Append(totalMontoCell);
            tVentas.Append(totalRowV);
            body.Append(tVentas);
        }

        // Detalle Compras
        var detalleCompras = dto.DetalleCompras ?? new List<DetalleCompraInforme>();
        body.Append(new Paragraph(
            new ParagraphProperties(new ParagraphStyleId() { Val = "Heading2" }),
            new Run(new Text("Detalle de Compras") { Space = SpaceProcessingModeValues.Preserve })
        ));
        if (detalleCompras.Count == 0)
        {
            body.Append(new Paragraph(new Run(new Text("Sin movimientos de compra en el periodo."))));
        }
        else
        {
            var tCompras = CreateTable("Producto", "Cant.", "Costo Unit.", "Subtotal");
            foreach (var d in detalleCompras.Take(TakeCap))
            {
                var row = new TableRow();
                row.Append(Cell(d.Producto));
                row.Append(Cell(d.Cantidad.ToString()));
                row.Append(Cell(FormatCurrency(d.CostoUnitario)));
                row.Append(Cell(FormatCurrency(d.Subtotal)));
                tCompras.Append(row);
            }
            var totalRowC = new TableRow();
            var totalCellLabelC = Cell("TOTAL");
            totalCellLabelC.TableCellProperties = new TableCellProperties(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "E8E8E8" });
            totalRowC.Append(totalCellLabelC);
            var totalCantCellC = Cell(detalleCompras.Sum(x => x.Cantidad).ToString());
            totalCantCellC.TableCellProperties = new TableCellProperties(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "E8E8E8" });
            totalRowC.Append(totalCantCellC);
            var emptyCellC = Cell("");
            emptyCellC.TableCellProperties = new TableCellProperties(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "E8E8E8" });
            totalRowC.Append(emptyCellC);
            var totalMontoCellC = Cell(FormatCurrency(dto.ComprasTotales));
            totalMontoCellC.TableCellProperties = new TableCellProperties(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "E8E8E8" });
            totalRowC.Append(totalMontoCellC);
            tCompras.Append(totalRowC);
            body.Append(tCompras);
        }
    }

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

    private static Table BuildIngresosGastosSummary(IngresosGastosResponse dto)
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
