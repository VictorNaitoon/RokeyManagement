using API.DTO.Response;
using API.DTO.Response.Suscripcion;
using API.Services.Common;
using API.Services.Suscripcion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Suscripcion
{
    [ApiController]
    [Route("api/v1/pagos")]
    [Authorize]
    public class PagoSuscripcionController : ControllerBase
    {
        private readonly IPagoSuscripcionService _pagoSuscripcionService;
        private readonly ICurrentUserService _currentUser;

        public PagoSuscripcionController(
            IPagoSuscripcionService pagoSuscripcionService,
            ICurrentUserService currentUser)
        {
            _pagoSuscripcionService = pagoSuscripcionService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene el historial de pagos de suscripción del negocio
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<PagoSuscripcionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetHistorialPagos()
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new ErrorResponse { Error = "El super administrador no puede ver pagos de un negocio" });
            }

            var pagos = await _pagoSuscripcionService.GetPagosByNegocioAsync(_currentUser.NegocioId);
            return Ok(pagos);
        }

        /// <summary>
        /// Obtiene los detalles de un pago específico
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PagoSuscripcionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPagoById(int id)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new ErrorResponse { Error = "El super administrador no puede ver pagos de un negocio" });
            }

            var pago = await _pagoSuscripcionService.GetPagoByIdAsync(id, _currentUser.NegocioId);
            if (pago == null)
            {
                return NotFound(new ErrorResponse { Error = "Pago no encontrado" });
            }
            return Ok(pago);
        }
    }
}
