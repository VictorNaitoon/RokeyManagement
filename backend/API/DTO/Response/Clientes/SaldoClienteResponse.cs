namespace API.DTO.Response.Clientes
{
    /// <summary>
    /// Respuesta de saldo pendiente de cliente
    /// </summary>
    public record SaldoClienteResponse(
        int IdCliente,
        string NombreCliente,
        decimal TotalVentasFiado,
        decimal TotalPagado,
        decimal SaldoPendiente,
        decimal LimiteCredito,
        decimal CreditoDisponible,
        bool PermiteFiado,
        int CantidadVentasPendientes,
        List<VentaResumenResponse> VentasPendientes
    );
}
