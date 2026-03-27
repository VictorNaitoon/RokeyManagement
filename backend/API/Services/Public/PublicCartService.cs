using System.Text.Json;
using API.Data;
using API.DTO.Request.Public;
using API.DTO.Response.Public;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Publicos
{
    /// <summary>
    /// Implementación del servicio de carrito basado en cookies firmadas
    /// </summary>
    public class CarritoPublicoService : ICarritoPublicoService
    {
        private readonly AppDbContext _context;
        private readonly IDataProtector _dataProtector;
        private const string CART_COOKIE_NAME = "carrito_publico";
        private const int COOKIE_EXPIRATION_DAYS = 7;

        public CarritoPublicoService(AppDbContext context, IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _dataProtector = dataProtectionProvider.CreateProtector("CarritoPublico_v1");
        }

        /// <inheritdoc/>
        public async Task<CartResponse> GetCartAsync(HttpRequest request, int idNegocio)
        {
            var cartData = GetCartFromCookie(request);
            if (cartData == null || cartData.IdNegocio != idNegocio)
            {
                return new CartResponse(new List<CartItemResponse>(), 0);
            }

            return await BuildCartResponseAsync(cartData);
        }

        /// <inheritdoc/>
        public async Task<CartResponse> AddItemAsync(HttpRequest request, HttpResponse response, int idNegocio, AgregarCarritoRequest requestDto)
        {
            var cartData = GetCartFromCookie(request) ?? new CartData { IdNegocio = idNegocio };

            // Verificar límite de 4KB
            var testJson = JsonSerializer.Serialize(cartData);
            if (testJson.Length > 3500)
            {
                throw new InvalidOperationException("Carrito lleno. Inicie sesión o finalice su compra.");
            }

            // Agregar o actualizar cantidad
            var existingItem = cartData.Items.FirstOrDefault(i => i.IdProducto == requestDto.IdProducto);
            if (existingItem != null)
            {
                existingItem.Cantidad += requestDto.Cantidad;
            }
            else
            {
                cartData.Items.Add(new CartItemData 
                { 
                    IdProducto = requestDto.IdProducto, 
                    Cantidad = requestDto.Cantidad 
                });
            }

            SaveCartToCookie(response, cartData);
            return await BuildCartResponseAsync(cartData);
        }

        /// <inheritdoc/>
        public async Task<CartResponse> UpdateItemAsync(HttpRequest request, HttpResponse response, int idNegocio, int productoId, int cantidad)
        {
            var cartData = GetCartFromCookie(request);
            if (cartData == null || cartData.IdNegocio != idNegocio)
            {
                return new CartResponse(new List<CartItemResponse>(), 0);
            }

            var existingItem = cartData.Items.FirstOrDefault(i => i.IdProducto == productoId);
            if (existingItem == null)
            {
                return await BuildCartResponseAsync(cartData);
            }

            if (cantidad <= 0)
            {
                cartData.Items.Remove(existingItem);
            }
            else
            {
                existingItem.Cantidad = cantidad;
            }

            SaveCartToCookie(response, cartData);
            return await BuildCartResponseAsync(cartData);
        }

        /// <inheritdoc/>
        public Task ClearCartAsync(HttpResponse response)
        {
            response.Cookies.Delete(CART_COOKIE_NAME);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public string GetOrCreateSessionId(HttpRequest request, HttpResponse response)
        {
            var cartData = GetCartFromCookie(request);
            if (cartData == null)
            {
                cartData = new CartData();
                SaveCartToCookie(response, cartData);
            }
            return cartData.SessionId ?? Guid.NewGuid().ToString();
        }

        // Métodos privados para manejo de cookies

        private CartData? GetCartFromCookie(HttpRequest request)
        {
            var cookieValue = request.Cookies[CART_COOKIE_NAME];
            if (string.IsNullOrEmpty(cookieValue))
            {
                return null;
            }

            try
            {
                var decrypted = _dataProtector.Unprotect(cookieValue);
                return JsonSerializer.Deserialize<CartData>(decrypted);
            }
            catch
            {
                return null;
            }
        }

        private void SaveCartToCookie(HttpResponse response, CartData cartData)
        {
            var json = JsonSerializer.Serialize(cartData);
            var encrypted = _dataProtector.Protect(json);
            
            response.Cookies.Append(CART_COOKIE_NAME, encrypted, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(COOKIE_EXPIRATION_DAYS),
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false // Cambiar a true en producción con HTTPS
            });
        }

        private async Task<CartResponse> BuildCartResponseAsync(CartData cartData)
        {
            if (!cartData.Items.Any())
            {
                return new CartResponse(new List<CartItemResponse>(), 0);
            }

            var productIds = cartData.Items.Select(i => i.IdProducto).ToList();
            
            var products = await _context.Productos
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var items = new List<CartItemResponse>();
            foreach (var item in cartData.Items)
            {
                if (products.TryGetValue(item.IdProducto, out var product))
                {
                    var subtotal = product.PrecioVenta * item.Cantidad;
                    items.Add(new CartItemResponse(
                        item.IdProducto,
                        product.Nombre,
                        product.PrecioVenta,
                        item.Cantidad,
                        subtotal
                    ));
                }
            }

            var total = items.Sum(i => i.Subtotal);
            return new CartResponse(items, total);
        }

        // Clases internas para serialización del carrito
        private class CartData
        {
            public string? SessionId { get; set; } = Guid.NewGuid().ToString();
            public int IdNegocio { get; set; }
            public List<CartItemData> Items { get; set; } = new();
        }

        private class CartItemData
        {
            public int IdProducto { get; set; }
            public int Cantidad { get; set; }
        }
    }
}