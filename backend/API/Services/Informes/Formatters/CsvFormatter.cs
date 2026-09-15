using System.Text;
using API.Data;
using API.DTO.Response.Informes;
using API.Models;

namespace API.Services.Informes.Formatters;

public class CsvFormatter
{
    private const int TakeCap = 5000;

    public Task<ExportResult> FormatAsync(string tipo, object dto, string periodo, Negocio negocio, CancellationToken ct)
    {
        var filename = $"informe-{tipo}-{periodo}.csv";
        var contentType = "text/csv; charset=utf-8";

        using var ms = new MemoryStream();
        // UTF-8 BOM
        ms.Write(new byte[] { 0xEF, 0xBB, 0xBF }, 0, 3);

        using var writer = new StreamWriter(ms, new UTF8Encoding(false), leaveOpen: true);
        WriteContent(tipo.ToLowerInvariant(), dto, writer);
        writer.Flush();
        var bytes = ms.ToArray();
        var result = new ExportResult(bytes, contentType, filename);
        return Task.FromResult(result);
    }

    private static void WriteContent(string tipo, object dto, StreamWriter writer)
    {
        switch (tipo)
        {
            case "ventas-resumen":
                WriteVentasResumen((VentasResumenResponse)dto, writer);
                break;
            case "productos-top":
                WriteProductosTop((ProductosTopResponse)dto, writer);
                break;
            case "flujo-caja":
                WriteFlujoCaja((FlujoCajaResponse)dto, writer);
                break;
            case "ingresos-gastos":
                WriteIngresosGastos((IngresosGastosResponse)dto, writer);
                break;
            case "alertas-stock":
                WriteAlertasStock((AlertasStockResponse)dto, writer);
                break;
            case "ventas-por-pago":
                WriteVentasPorPago((VentasPorPagoResponse)dto, writer);
                break;
            case "ventas-por-vendedor":
                WriteVentasPorVendedor((VentasPorVendedorResponse)dto, writer);
                break;
            default:
                throw new ArgumentException($"Tipo desconocido: {tipo}", nameof(tipo));
        }
    }

    private static void WriteVentasResumen(VentasResumenResponse dto, StreamWriter w)
    {
        WriteRow(w, ["TotalVentas", "CantidadVentas", "TicketPromedio", "VentasAnuladas", "Periodo"]);
        WriteRow(w, [FormatDecimal(dto.TotalVentas), dto.CantidadVentas.ToString(), FormatDecimal(dto.TicketPromedio), dto.VentasAnuladas.ToString(), dto.Periodo]);
    }

    private static void WriteProductosTop(ProductosTopResponse dto, StreamWriter w)
    {
        WriteRow(w, ["Producto", "Cantidad", "MontoTotal"]);
        var items = dto.Productos.Take(TakeCap);
        foreach (var p in items)
            WriteRow(w, [p.Nombre, p.CantidadVendida.ToString(), FormatDecimal(p.MontoTotal)]);
    }

    private static void WriteFlujoCaja(FlujoCajaResponse dto, StreamWriter w)
    {
        WriteRow(w, ["Ingresos", "Egresos", "Balance", "MovimientosIngreso", "MovimientosEgreso"]);
        WriteRow(w, [FormatDecimal(dto.Ingresos), FormatDecimal(dto.Egresos), FormatDecimal(dto.Balance), dto.MovimientosIngreso.ToString(), dto.MovimientosEgreso.ToString()]);
    }

    private static void WriteIngresosGastos(IngresosGastosResponse dto, StreamWriter w)
    {
        WriteRow(w, ["VentasTotales", "ComprasTotales", "GananciaBruta", "MargenPorcentaje"]);
        WriteRow(w, [FormatDecimal(dto.VentasTotales), FormatDecimal(dto.ComprasTotales), FormatDecimal(dto.GananciaBruta), dto.MargenPorcentaje.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)]);
    }

    private static void WriteAlertasStock(AlertasStockResponse dto, StreamWriter w)
    {
        WriteRow(w, ["Producto", "StockActual", "StockMinimo", "Diferencia"]);
        var items = dto.Productos.Take(TakeCap);
        foreach (var p in items)
            WriteRow(w, [p.Nombre, p.StockActual.ToString(), p.StockMinimo.ToString(), p.Diferencia.ToString()]);
    }

    private static void WriteVentasPorPago(VentasPorPagoResponse dto, StreamWriter w)
    {
        WriteRow(w, ["Metodo", "Cantidad", "Monto", "Porcentaje"]);
        var items = dto.Metodos.Take(TakeCap);
        foreach (var m in items)
            WriteRow(w, [m.Metodo, m.Cantidad.ToString(), FormatDecimal(m.Monto), m.Porcentaje.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)]);
    }

    private static void WriteVentasPorVendedor(VentasPorVendedorResponse dto, StreamWriter w)
    {
        WriteRow(w, ["Vendedor", "CantidadVentas", "MontoTotal"]);
        var items = dto.Vendedores.Take(TakeCap);
        foreach (var v in items)
            WriteRow(w, [v.Nombre, v.CantidadVentas.ToString(), FormatDecimal(v.MontoTotal)]);
    }

    private static void WriteRow(StreamWriter w, string[] fields)
    {
        var escaped = fields.Select(Escape);
        w.WriteLine(string.Join(",", escaped));
    }

    private static string Escape(string field)
    {
        if (field == null) return "";
        var needsQuotes = field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r');
        if (field.Contains('"'))
            field = field.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{field}\"" : field;
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
    }
}
