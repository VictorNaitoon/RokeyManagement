namespace API.DTO.Response.Public
{
    /// <summary>
    /// Producto visible en el catálogo público (sin precio de costo ni stock interno)
    /// </summary>
    public record ProductoPublicResponse(
        int Id,
        string Nombre,
        string? Descripcion,
        decimal PrecioVenta,
        string? ImagenURL,
        int StockActual
    );
}