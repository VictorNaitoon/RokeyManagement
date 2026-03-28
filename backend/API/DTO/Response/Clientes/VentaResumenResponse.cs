namespace API.DTO.Response.Clientes
{
    /// <summary>
    /// Resumen de venta para historial de cliente
    /// </summary>
    public record VentaResumenResponse(
        int Id,
        DateTime FechaVenta,
        decimal TotalVenta,
        string Estado,
        int CantidadItems,
        List<string> MetodosPago
    );
}
