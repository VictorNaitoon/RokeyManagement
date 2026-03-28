namespace API.DTO.Response.Clientes
{
    /// <summary>
    /// Respuesta de pago de cliente
    /// </summary>
    public record PagoClienteResponse(
        int IdPago,
        int IdVenta,
        DateTime FechaVenta,
        string MetodoPago,
        decimal Monto,
        string? NumeroComprobante
    );
}
