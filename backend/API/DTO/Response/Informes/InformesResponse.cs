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
    /// Line item for sales detail (used inside ingresos-gastos)
    /// </summary>
    public record DetalleVentaInforme(
        string Producto,
        int Cantidad,
        decimal PrecioUnitario,
        decimal Subtotal
    );

    /// <summary>
    /// Line item for purchase detail (used inside ingresos-gastos)
    /// </summary>
    public record DetalleCompraInforme(
        string Producto,
        int Cantidad,
        decimal CostoUnitario,
        decimal Subtotal
    );

    /// <summary>
    /// Response DTO for revenue vs expenses (ingresos-gastos).
    /// Enriched with itemised detail for ventas and compras — totals remain, detail is additive.
    /// </summary>
    public record IngresosGastosResponse(
        decimal VentasTotales,
        decimal ComprasTotales,
        decimal GananciaBruta,
        decimal MargenPorcentaje,
        List<DetalleVentaInforme>? DetalleVentas = null,
        List<DetalleCompraInforme>? DetalleCompras = null
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
