using API.DTO.Request.Suscripcion;
using API.DTO.Response;
using API.DTO.Response.Suscripcion;
using API.Services.Common;
using API.Services.Suscripcion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Suscripcion
{
    [ApiController]
    [Route("api/v1/suscripcion")]
    [Authorize]
    public class SuscripcionController : ControllerBase
    {
        private readonly ISuscripcionService _suscripcionService;
        private readonly IMetricaUsoService _metricaUsoService;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<SuscripcionController> _logger;

        public SuscripcionController(
            ISuscripcionService suscripcionService,
            IMetricaUsoService metricaUsoService,
            ICurrentUserService currentUser,
            ILogger<SuscripcionController> logger)
        {
            _suscripcionService = suscripcionService;
            _metricaUsoService = metricaUsoService;
            _currentUser = currentUser;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la suscripción actual del negocio autenticado
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(SuscripcionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSuscripcion()
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new ErrorResponse { Error = "El super administrador no puede ver suscripciones de un negocio" });
            }

            var suscripcion = await _suscripcionService.GetSuscripcionByNegocioAsync(_currentUser.NegocioId);
            if (suscripcion == null)
            {
                return NotFound(new ErrorResponse { Error = "No se encontró suscripción activa para este negocio" });
            }

            return Ok(suscripcion);
        }

        /// <summary>
        /// Crea una nueva suscripción para el negocio (onboarding)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SuscripcionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateSuscripcion([FromBody] SuscripcionRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new ErrorResponse { Error = "El super administrador no puede crear suscripciones" });
            }

            try
            {
                var suscripcion = await _suscripcionService.CreateSuscripcionAsync(_currentUser.NegocioId, request);
                return CreatedAtAction(nameof(GetSuscripcion), suscripcion);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza la suscripción (upgrade/downgrade de plan)
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(SuscripcionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateSuscripcion([FromBody] SuscripcionRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new ErrorResponse { Error = "El super administrador no puede actualizar suscripciones" });
            }

            try
            {
                var suscripcion = await _suscripcionService.UpdateSuscripcionAsync(_currentUser.NegocioId, request);
                return Ok(suscripcion);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }

        /// <summary>
        /// Cancela la suscripción del negocio
        /// </summary>
        [HttpPost("cancel")]
        [ProducesResponseType(typeof(SuscripcionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelSuscripcion([FromBody] CancelarSuscripcionRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new ErrorResponse { Error = "El super administrador no puede cancelar suscripciones" });
            }

            try
            {
                var suscripcion = await _suscripcionService.CancelarSuscripcionAsync(_currentUser.NegocioId, request);
                return Ok(suscripcion);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica los límites del plan vs uso actual
        /// </summary>
        [HttpGet("limites")]
        [ProducesResponseType(typeof(CheckLimitesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetLimites()
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new ErrorResponse { Error = "El super administrador no puede verificar límites" });
            }

            try
            {
                var limites = await _suscripcionService.CheckLimitesAsync(_currentUser.NegocioId);
                return Ok(limites);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene las métricas de uso actuales del negocio
        /// </summary>
        [HttpGet("uso")]
        [ProducesResponseType(typeof(MetricaUsoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUso()
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new ErrorResponse { Error = "El super administrador no puede ver métricas de uso" });
            }

            var metricas = await _metricaUsoService.GetMetricaActualAsync(_currentUser.NegocioId);
            return Ok(metricas);
        }
    }
}
