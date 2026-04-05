namespace API.DTO.Response.Public
{
    /// <summary>
    /// Item individual del carrito público
    /// </summary>
    public record CartItemResponse(
        int IdProducto,
        string Nombre,
        decimal PrecioVenta,
        int Cantidad,
        decimal Subtotal
    );
}