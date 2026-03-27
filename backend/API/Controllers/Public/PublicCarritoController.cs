using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTO.Request.Public;
using API.Services.Publicos;

namespace API.Controllers.Publicos
{
    [ApiController]
    [Route("api/v1/publico/carrito")]
    [AllowAnonymous]
    public class CarritoPublicoController : ControllerBase
    {
        private readonly ICarritoPublicoService _carritoService;

        public CarritoPublicoController(ICarritoPublicoService carritoService)
        {
            _carritoService = carritoService;
        }

        /// <summary>
        /// Obtiene el carrito actual desde la cookie
        /// </summary>
        /// <param name="idNegocio">ID del negocio</param>
        [HttpGet]
        [ProducesResponseType(typeof(DTO.Response.Public.CartResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCarrito([FromQuery] int idNegocio)
        {
            if (idNegocio <= 0)
            {
                return BadRequest(new { error = "Se requiere el ID del negocio" });
            }

            var cart = await _carritoService.GetCartAsync(Request, idNegocio);
            return Ok(cart);
        }

        /// <summary>
        /// Agrega un producto al carrito
        /// </summary>
        /// <param name="idNegocio">ID del negocio</param>
        [HttpPost]
        [ProducesResponseType(typeof(DTO.Response.Public.CartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AgregarItem([FromQuery] int idNegocio, [FromBody] AgregarCarritoRequest request)
        {
            if (idNegocio <= 0)
            {
                return BadRequest(new { error = "Se requiere el ID del negocio" });
            }

            try
            {
                var cart = await _carritoService.AddItemAsync(Request, Response, idNegocio, request);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza la cantidad de un producto en el carrito
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="idNegocio">ID del negocio</param>
        /// <param name="cantidad">Nueva cantidad</param>
        [HttpPut("{productoId}")]
        [ProducesResponseType(typeof(DTO.Response.Public.CartResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ActualizarItem(int productoId, [FromQuery] int idNegocio, [FromQuery] int cantidad)
        {
            if (idNegocio <= 0)
            {
                return BadRequest(new { error = "Se requiere el ID del negocio" });
            }

            var cart = await _carritoService.UpdateItemAsync(Request, Response, idNegocio, productoId, cantidad);
            return Ok(cart);
        }

        /// <summary>
        /// Vacía el carrito (elimina la cookie)
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> VaciarCarrito()
        {
            await _carritoService.ClearCartAsync(Response);
            return Ok(new { message = "Carrito vaciado" });
        }
    }
}