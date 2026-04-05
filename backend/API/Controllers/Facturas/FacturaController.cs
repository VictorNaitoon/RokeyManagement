using API.DTO.Request.Facturas;
using API.DTO.Response.Facturas;
using API.Services.Facturas;
using API.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Facturas
{
    [ApiController]
    [Route("api/v1/facturas")]
    [Authorize]
    public class FacturaController : ControllerBase
    {
        private readonly IFacturaService _facturaService;
        private readonly ICurrentUserService _currentUser;

        public FacturaController(IFacturaService facturaService, ICurrentUserService currentUser)
        {
            _facturaService = facturaService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene todas las facturas del negocio con paginación y filtros opcionales
        /// </summary>
        /// <param name="page">Número de página (default 1)</param>
        /// <param name="pageSize">Tamaño de página (default 20)</param>
        /// <param name="fechaDesde">Fecha inicial del filtro (opcional)</param>
        /// <param name="fechaHasta">Fecha final del filtro (opcional)</param>
        /// <returns>Lista paginada de facturas</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ListadoFacturaResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ObtenerTodasLasFacturas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar facturas de un negocio" });
            }

            var result = await _facturaService.ObtenerTodasLasFacturasAsync(
                page, pageSize, fechaDesde, fechaHasta, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una factura específica por ID
        /// </summary>
        /// <param name="id">ID de la factura</param>
        /// <returns>Datos de la factura</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FacturaResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ObtenerFacturaPorId(int id, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar facturas de un negocio" });
            }

            var factura = await _facturaService.ObtenerFacturaPorIdAsync(id, ct);
            
            if (factura == null)
            {
                return NotFound(new { message = "Factura no encontrada" });
            }

            return Ok(factura);
        }

        /// <summary>
        /// Crea una factura proforma asociada a una venta existente
        /// </summary>
        /// <param name="request">Datos de la factura a crear</param>
        /// <returns>Factura creada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(FacturaResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> CrearFacturaProforma([FromBody] CrearFacturaRequest request, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar facturas de un negocio" });
            }

            // Verificar roles: Admin || Manager || Vendedor pueden crear facturas
            if (!(_currentUser.IsAdmin || _currentUser.IsManager || _currentUser.IsVendedor))
            {
                return StatusCode(403, new { message = "No tiene permisos para crear facturas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _facturaService.CrearFacturaProformaAsync(request, ct);
                return CreatedAtAction(nameof(ObtenerFacturaPorId), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                // Distinguir entre diferentes tipos de errores
                if (ex.Message.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { message = ex.Message });
                }
                if (ex.Message.Contains("ya posee", StringComparison.OrdinalIgnoreCase) || 
                    ex.Message.Contains("ya tiene", StringComparison.OrdinalIgnoreCase))
                {
                    return Conflict(new { message = ex.Message });
                }
                return BadRequest(new { message = ex.Message });
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Emite una nota de crédito asociada a una venta con factura oficial (con CAE)
        /// </summary>
        /// <param name="request">Datos de la nota de crédito</param>
        /// <returns>Nota de crédito creada</returns>
        [HttpPost("nota-credito")]
        [ProducesResponseType(typeof(FacturaResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> EmitirNotaCredito([FromBody] NotaCreditoRequest request, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar facturas de un negocio" });
            }

            // Verificar roles: Admin || Manager || Vendedor pueden emitir notas de crédito
            if (!(_currentUser.IsAdmin || _currentUser.IsManager || _currentUser.IsVendedor))
            {
                return StatusCode(403, new { message = "No tiene permisos para emitir notas de crédito" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _facturaService.EmitirNotaCreditoAsync(request, ct);
                return CreatedAtAction(nameof(ObtenerFacturaPorId), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { message = ex.Message });
                }
                if (ex.Message.Contains("no tiene una factura", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("sin factura", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = ex.Message });
                }
                return BadRequest(new { message = ex.Message });
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Valida que una venta no tenga una factura asociada
        /// </summary>
        /// <param name="idVenta">ID de la venta a validar</param>
        /// <returns>True si no tiene factura, False si ya tiene</returns>
        [HttpGet("ventas/{idVenta}/sin-factura")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ValidarVentaSinFactura(int idVenta, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar facturas de un negocio" });
            }

            try
            {
                var resultado = await _facturaService.ValidarVentaSinFacturaAsync(idVenta, ct);
                return Ok(new { tieneFactura = !resultado });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { message = ex.Message });
                }
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
