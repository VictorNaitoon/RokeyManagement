using API.DTO.Request.Caja;
using API.DTO.Response.Caja;
using API.Services.Caja;
using API.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Caja
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class CajaController : ControllerBase
    {
        private readonly ICajaService _cajaService;
        private readonly ICurrentUserService _currentUser;

        public CajaController(ICajaService cajaService, ICurrentUserService currentUser)
        {
            _cajaService = cajaService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Abre una nueva caja para el negocio
        /// </summary>
        [HttpPost("apertura")]
        [ProducesResponseType(typeof(CajaResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> AbrirCaja([FromBody] AperturaCajaRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar cajas de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "Solo el dueño o gerente pueden abrir la caja" });
            }

            try
            {
                var result = await _cajaService.AbrirCajaAsync(
                    request,
                    _currentUser.UserId,
                    _currentUser.NegocioId);

                return CreatedAtAction(nameof(ObtenerEstadoCaja), new { }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cierra la caja abierta del negocio
        /// </summary>
        [HttpPost("cierre")]
        [ProducesResponseType(typeof(CajaResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CerrarCaja([FromBody] CierreCajaRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar cajas de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "Solo el dueño o gerente pueden cerrar la caja" });
            }

            try
            {
                var result = await _cajaService.CerrarCajaAsync(
                    request,
                    _currentUser.UserId,
                    _currentUser.NegocioId);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el estado de caja del negocio (abierta o cerrada)
        /// </summary>
        [HttpGet("actual")]
        [ProducesResponseType(typeof(EstadoCajaResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ObtenerEstadoCaja()
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar cajas de un negocio" });
            }

            var result = await _cajaService.ObtenerEstadoCajaAsync(_currentUser.NegocioId);

            return Ok(result);
        }

        /// <summary>
        /// Agrega un movimiento (ingreso o egreso) a la caja abierta
        /// </summary>
        [HttpPost("movimientos")]
        [ProducesResponseType(typeof(MovimientoCajaResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AgregarMovimiento([FromBody] AgregarMovimientoCajaRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar cajas de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager && !_currentUser.IsVendedor)
            {
                return StatusCode(403, new { message = "Solo el dueño, gerente o empleado pueden registrar movimientos de caja" });
            }

            try
            {
                var result = await _cajaService.AgregarMovimientoAsync(
                    request,
                    _currentUser.UserId,
                    _currentUser.NegocioId);

                return CreatedAtAction(nameof(ObtenerMovimientos), new { id = result.Id_caja }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los movimientos de una caja específica
        /// </summary>
        [HttpGet("{id}/movimientos")]
        [ProducesResponseType(typeof(IEnumerable<MovimientoCajaResponse>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ObtenerMovimientos(int id)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar cajas de un negocio" });
            }

            var result = await _cajaService.ObtenerMovimientosAsync(id);

            return Ok(result);
        }
    }
}
