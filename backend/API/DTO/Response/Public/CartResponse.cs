namespace API.DTO.Response.Public
{
    /// <summary>
    /// Respuesta completa del carrito público
    /// </summary>
    public record CartResponse(
        List<CartItemResponse> Items,
        decimal Total
    );
}