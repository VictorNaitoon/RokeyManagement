namespace API.DTO.Request.Public
{
    /// <summary>
    /// Request para crear un nuevo cliente fiado
    /// </summary>
    public record CrearClienteRequest(
        string Nombre,
        string? Apellido,
        string? Documento,
        string? CondicionIVA,
        string? Telefono,
        string? Email,
        string? Direccion,
        bool PermiteFiado = false,
        decimal? LimiteCredito = null
    );
}