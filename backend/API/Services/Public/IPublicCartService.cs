using API.DTO.Request.Public;
using API.DTO.Response.Public;
using Microsoft.AspNetCore.Http;

namespace API.Services.Publicos
{
    /// <summary>
    /// Servicio para el carrito basado en cookies (usuarios anonymous)
    /// </summary>
    public interface ICarritoPublicoService
    {
        /// <summary>
        /// Obtiene el carrito desde la cookie
        /// </summary>
        Task<CartResponse> GetCartAsync(HttpRequest request, int idNegocio);
        
        /// <summary>
        /// Agrega un producto al carrito en cookie
        /// </summary>
        Task<CartResponse> AddItemAsync(HttpRequest request, HttpResponse response, int idNegocio, AgregarCarritoRequest requestDto);
        
        /// <summary>
        /// Actualiza la cantidad de un producto en el carrito
        /// </summary>
        Task<CartResponse> UpdateItemAsync(HttpRequest request, HttpResponse response, int idNegocio, int productoId, int cantidad);
        
        /// <summary>
        /// Vacía el carrito (elimina la cookie)
        /// </summary>
        Task ClearCartAsync(HttpResponse response);
        
        /// <summary>
        /// Obtiene el ID de sesión del carrito desde la cookie o genera uno nuevo
        /// </summary>
        string GetOrCreateSessionId(HttpRequest request, HttpResponse response);
    }
}