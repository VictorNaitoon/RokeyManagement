using API.DTO.Request.CarritoInterno;
using API.DTO.Response.CarritoInterno;
using API.DTO.Response.Ventas;
using API.Services.Common;
using API.Services.CarritoInterno;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.CarritoInterno
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class CarritoInternoController : ControllerBase
    {
        private readonly ICarritoInternoService _service;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<CarritoInternoController> _logger;

        public CarritoInternoController(
            ICarritoInternoService service,
            ICurrentUserService currentUser,
            ILogger<CarritoInternoController> logger)
        {
            _service = service;
            _currentUser = currentUser;
            _logger = logger;
        }

        /// <summary>
        /// Crea un nuevo carrito interno.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CarritoInternoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Crear([FromBody] CreateCarritoInternoRequest request, CancellationToken ct)
        {
            if (_currentUser.IsSuperAdmin)
                return StatusCode(403, new { message = "El super administrador no puede gestionar carritos internos de un negocio" });

            var result = await _service.CrearAsync(request, ct);
            return CreatedAtAction(nameof(Obtener), new { id = result.Id }, result);
        }

        /// <summary>
        /// Obtiene un carrito interno por su ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CarritoInternoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Obtener(int id, CancellationToken ct)
        {
            if (_currentUser.IsSuperAdmin)
                return StatusCode(403, new { message = "El super administrador no puede gestionar carritos internos de un negocio" });

            var result = await _service.ObtenerPorIdAsync(id, ct);
            if (result == null)
            {
                return NotFound(new { error = "Carrito no encontrado" });
            }
            return Ok(result);
        }

        /// <summary>
        /// Lista los carritos activos del vendedor autenticado.
        /// </summary>
        [HttpGet("activos")]
        [ProducesResponseType(typeof(List<CarritoInternoResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarActivos(CancellationToken ct)
        {
            if (_currentUser.IsSuperAdmin)
                return StatusCode(403, new { message = "El super administrador no puede gestionar carritos internos de un negocio" });

            var result = await _service.ListarActivosAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Agrega un item al carrito.
        /// </summary>
        [HttpPost("{id}/items")]
        [ProducesResponseType(typeof(CarritoInternoItemResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AgregarItem(int id, [FromBody] AgregarItemRequest request, CancellationToken ct)
        {
            if (_currentUser.IsSuperAdmin)
                return StatusCode(403, new { message = "El super administrador no puede gestionar carritos internos de un negocio" });

            var result = await _service.AgregarItemAsync(id, request, ct);
            return CreatedAtAction(nameof(Obtener), new { id = id }, result);
        }

        /// <summary>
        /// Actualiza la cantidad de un item del carrito.
        /// </summary>
        [HttpPut("{id}/items/{itemId}")]
        [ProducesResponseType(typeof(CarritoInternoItemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarItem(int id, int itemId, [FromBody] UpdateItemRequest request, CancellationToken ct)
        {
            if (_currentUser.IsSuperAdmin)
                return StatusCode(403, new { message = "El super administrador no puede gestionar carritos internos de un negocio" });

            var result = await _service.ActualizarItemAsync(id, itemId, request, ct);
            return Ok(result);
        }

        /// <summary>
        /// Elimina un item del carrito.
        /// </summary>
        [HttpDelete("{id}/items/{itemId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarItem(int id, int itemId, CancellationToken ct)
        {
            if (_currentUser.IsSuperAdmin)
                return StatusCode(403, new { message = "El super administrador no puede gestionar carritos internos de un negocio" });

            await _service.EliminarItemAsync(id, itemId, ct);
            return NoContent();
        }

        /// <summary>
        /// Elimina el carrito.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
        {
            if (_currentUser.IsSuperAdmin)
                return StatusCode(403, new { message = "El super administrador no puede gestionar carritos internos de un negocio" });

            await _service.EliminarAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Convierte el carrito a una venta.
        /// </summary>
        [HttpPost("{id}/convertir")]
        [ProducesResponseType(typeof(VentaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Convertir(int id, [FromBody] ConvertirCarritoRequest request, CancellationToken ct)
        {
            if (_currentUser.IsSuperAdmin)
                return StatusCode(403, new { message = "El super administrador no puede gestionar carritos internos de un negocio" });

            var result = await _service.ConvertirAsync(id, request, ct);
            return Ok(result);
        }
    }
}
