namespace API.DTO.Request.Public
{
    /// <summary>
    /// Request para agregar un producto al carrito público
    /// </summary>
    public record AgregarCarritoRequest(
        int IdProducto,
        int Cantidad
    );
}