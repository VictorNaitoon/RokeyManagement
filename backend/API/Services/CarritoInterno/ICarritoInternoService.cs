using API.DTO.Request.CarritoInterno;
using API.DTO.Response.CarritoInterno;

namespace API.Services.CarritoInterno
{
    public interface ICarritoInternoService
    {
        /// <summary>
        /// Crea un nuevo carrito interno para el vendedor autenticado.
        /// </summary>
        Task<CarritoInternoResponse> CrearAsync(API.DTO.Request.CarritoInterno.CreateCarritoInternoRequest request, CancellationToken ct);

        /// <summary>
        /// Obtiene un carrito interno por su ID.
        /// </summary>
        Task<CarritoInternoResponse?> ObtenerPorIdAsync(int id, CancellationToken ct);

        /// <summary>
        /// Lista los carritos activos del vendedor autenticado.
        /// </summary>
        Task<List<CarritoInternoResponse>> ListarActivosAsync(CancellationToken ct);

        /// <summary>
        /// Agrega un item al carrito.
        /// </summary>
        Task<CarritoInternoItemResponse> AgregarItemAsync(int carritoId, AgregarItemRequest request, CancellationToken ct);

        /// <summary>
        /// Actualiza la cantidad de un item del carrito.
        /// </summary>
        Task<CarritoInternoItemResponse> ActualizarItemAsync(int carritoId, int itemId, UpdateItemRequest request, CancellationToken ct);

        /// <summary>
        /// Elimina un item del carrito.
        /// </summary>
        Task EliminarItemAsync(int carritoId, int itemId, CancellationToken ct);

        /// <summary>
        /// Elimina el carrito (solo si no está convertido).
        /// </summary>
        Task EliminarAsync(int carritoId, CancellationToken ct);

        /// <summary>
        /// Convierte el carrito a una venta.
        /// </summary>
        Task<API.DTO.Response.Ventas.VentaResponse> ConvertirAsync(int carritoId, ConvertirCarritoRequest request, CancellationToken ct);
    }
}
