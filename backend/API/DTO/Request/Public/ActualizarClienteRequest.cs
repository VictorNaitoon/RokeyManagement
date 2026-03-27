namespace API.DTO.Request.Public
{
    /// <summary>
    /// Request para actualizar un cliente fiado
    /// </summary>
    public record ActualizarClienteRequest(
        string? Nombre,
        string? Apellido,
        string? Documento,
        string? CondicionIVA,
        string? Telefono,
        string? Email,
        string? Direccion,
        bool? PermiteFiado,
        decimal? LimiteCredito
    );
}