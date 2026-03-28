namespace API.DTO.Response.Clientes
{
    /// <summary>
    /// Respuesta de cuenta de cliente
    /// </summary>
    public record AccountResponse(
        int Id,
        string Nombre,
        string? Apellido,
        string Email,
        string? Documento,
        string? CondicionIVA,
        string? Telefono,
        string? Direccion,
        bool PermiteFiado,
        decimal? LimiteCredito,
        bool Activo
    );
}
