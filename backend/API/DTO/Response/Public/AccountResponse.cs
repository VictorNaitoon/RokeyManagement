namespace API.DTO.Response.Public
{
    /// <summary>
    /// Respuesta de cuenta de cliente
    /// </summary>
    public record AccountResponse(
        int Id,
        string Nombre,
        string? Apellido,
        string Email,
        string? Token,
        ClienteInfoResponse? Info
    );

    /// <summary>
    /// Información adicional del cliente fiado
    /// </summary>
    public record ClienteInfoResponse(
        bool PermiteFiado,
        decimal? LimiteCredito
    );
}