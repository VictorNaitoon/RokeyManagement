using API.DTO.Request.Ventas;
using API.DTO.Response.Ventas;
using API.Services.Ventas;
using API.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Ventas
{
    [ApiController]
    [Route("api/v1/ventas")]
    [Authorize]
    public class VentaController : ControllerBase
    {
        private readonly IVentaService _ventaService;
        private readonly ICurrentUserService _currentUser;

        public VentaController(IVentaService ventaService, ICurrentUserService currentUser)
        {
            _ventaService = ventaService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene todas las ventas del negocio con paginación y filtros opcionales
        /// </summary>
        /// <param name="page">Número de página (default 1)</param>
        /// <param name="pageSize">Tamaño de página (default 20)</param>
        /// <param name="fechaDesde">Fecha inicial del filtro (opcional)</param>
        /// <param name="fechaHasta">Fecha final del filtro (opcional)</param>
        /// <returns>Lista paginada de ventas</returns>
        [HttpGet]
        [ProducesResponseType(typeof(VentaListResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ObtenerTodasLasVentas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar ventas de un negocio" });
            }

            var result = await _ventaService.ObtenerTodasLasVentasAsync(
                page, pageSize, fechaDesde, fechaHasta, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una venta específica por ID
        /// </summary>
        /// <param name="id">ID de la venta</param>
        /// <returns>Datos de la venta</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VentaResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ObtenerVentaPorId(int id, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar ventas de un negocio" });
            }

            var venta = await _ventaService.ObtenerVentaPorIdAsync(id, ct);
            
            if (venta == null)
            {
                return NotFound(new { message = "Venta no encontrada" });
            }

            return Ok(venta);
        }

        /// <summary>
        /// Registra una nueva venta con detalles y pagos
        /// </summary>
        /// <param name="request">Datos de la venta a crear</param>
        /// <returns>Venta creada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(VentaResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> CrearVenta([FromBody] CrearVentaRequest request, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar ventas de un negocio" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _ventaService.CrearVentaAsync(request, ct);
                return CreatedAtAction(nameof(ObtenerVentaPorId), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Anula una venta existente (solo administradores)
        /// </summary>
        /// <param name="id">ID de la venta a anular</param>
        /// <param name="request">Motivo de anulación (opcional)</param>
        /// <returns>Venta anulada</returns>
        [HttpPost("{id}/anular")]
        [Authorize(Roles = "Dueño")]
        [ProducesResponseType(typeof(VentaResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> AnularVenta(int id, [FromBody] AnularVentaRequest? request = null, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar ventas de un negocio" });
            }

            try
            {
                var result = await _ventaService.AnularVentaAsync(id, request ?? new AnularVentaRequest(), ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // Distinguir entre venta no encontrada (404) y conflicto de negocio (409)
                if (ex.Message.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { message = ex.Message });
                }
                // Conflicto de negocio (factura con CAE sin nota de crédito)
                return Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los detalles de una venta específica
        /// </summary>
        /// <param name="id">ID de la venta</param>
        /// <returns>Lista de detalles de la venta</returns>
        [HttpGet("{id}/detalles")]
        [ProducesResponseType(typeof(List<DetalleVentaResponse>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ObtenerDetallesVenta(int id, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar ventas de un negocio" });
            }

            try
            {
                var detalles = await _ventaService.ObtenerDetallesVentaAsync(id, ct);
                return Ok(detalles);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los pagos de una venta específica
        /// </summary>
        /// <param name="id">ID de la venta</param>
        /// <returns>Lista de pagos de la venta</returns>
        [HttpGet("{id}/pagos")]
        [ProducesResponseType(typeof(List<PagoResponse>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ObtenerPagosVenta(int id, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar ventas de un negocio" });
            }

            try
            {
                var pagos = await _ventaService.ObtenerPagosVentaAsync(id, ct);
                return Ok(pagos);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
