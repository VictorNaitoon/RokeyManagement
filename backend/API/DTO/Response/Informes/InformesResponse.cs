namespace API.DTO.Response.Informes
{
    /// <summary>
    /// Response DTO for sales summary (ventas-resumen)
    /// </summary>
    public record VentasResumenResponse(
        decimal TotalVentas,
        int CantidadVentas,
        decimal TicketPromedio,
        int VentasAnuladas,
        string Periodo
    );

    /// <summary>
    /// Response DTO for top products (productos-top)
    /// </summary>
    public record TopProductoResponse(
        int IdProducto,
        string Nombre,
        int CantidadVendida,
        decimal MontoTotal
    );

    public record ProductosTopResponse(
        List<TopProductoResponse> Productos
    );

    /// <summary>
    /// Response DTO for cash flow (flujo-caja)
    /// </summary>
    public record FlujoCajaResponse(
        decimal Ingresos,
        decimal Egresos,
        decimal Balance,
        int MovimientosIngreso,
        int MovimientosEgreso
    );

    /// <summary>
    /// Response DTO for revenue vs expenses (ingresos-gastos)
    /// </summary>
    public record IngresosGastosResponse(
        decimal VentasTotales,
        decimal ComprasTotales,
        decimal GananciaBruta,
        decimal MargenPorcentaje
    );

    /// <summary>
    /// Response DTO for stock alerts (alertas-stock)
    /// </summary>
    public record StockAlertResponse(
        int IdProducto,
        string Nombre,
        int StockActual,
        int StockMinimo,
        int Diferencia
    );

    public record AlertasStockResponse(
        List<StockAlertResponse> Productos
    );

    /// <summary>
    /// Response DTO for sales by payment method (ventas-por-pago)
    /// </summary>
    public record MetodoPagoResponse(
        string Metodo,
        int Cantidad,
        decimal Monto,
        decimal Porcentaje
    );

    public record VentasPorPagoResponse(
        List<MetodoPagoResponse> Metodos
    );

    /// <summary>
    /// Response DTO for sales by seller (ventas-por-vendedor)
    /// </summary>
    public record VendedorResponse(
        int IdUsuario,
        string Nombre,
        int CantidadVentas,
        decimal MontoTotal
    );

    public record VentasPorVendedorResponse(
        List<VendedorResponse> Vendedores
    );
}
