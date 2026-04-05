using API.DTO.Request.Presupuestos;
using API.DTO.Response.Presupuestos;
using API.DTO.Response.Ventas;
using API.Services.Presupuestos;
using API.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Presupuestos
{
    [ApiController]
    [Route("api/v1/presupuestos")]
    [Authorize]
    public class PresupuestosController : ControllerBase
    {
        private readonly IPresupuestoService _presupuestoService;
        private readonly ICurrentUserService _currentUser;

        public PresupuestosController(IPresupuestoService presupuestoService, ICurrentUserService currentUser)
        {
            _presupuestoService = presupuestoService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene todos los presupuestos del negocio con filtros opcionales
        /// </summary>
        /// <param name="estado">Filtrar por estado (opcional)</param>
        /// <param name="idCliente">Filtrar por cliente (opcional)</param>
        /// <returns>Lista de presupuestos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PresupuestoListResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAll(
            [FromQuery] Models.Enums.EstadoPresupuesto? estado = null,
            [FromQuery] int? idCliente = null,
            CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar presupuestos de un negocio" });
            }

            var result = await _presupuestoService.GetAllAsync(estado, idCliente, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un presupuesto específico por ID
        /// </summary>
        /// <param name="id">ID del presupuesto</param>
        /// <returns>Datos del presupuesto</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PresupuestoResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar presupuestos de un negocio" });
            }

            try
            {
                var presupuesto = await _presupuestoService.GetByIdAsync(id, ct);
                return Ok(presupuesto);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo presupuesto
        /// </summary>
        /// <param name="request">Datos del presupuesto a crear</param>
        /// <returns>Presupuesto creado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(PresupuestoResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Create([FromBody] CreatePresupuestoRequest request, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar presupuestos de un negocio" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _presupuestoService.CreateAsync(request, ct);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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
        /// Actualiza el estado de un presupuesto
        /// </summary>
        /// <param name="id">ID del presupuesto</param>
        /// <param name="request">Nuevo estado</param>
        /// <returns>Presupuesto actualizado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PresupuestoResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePresupuestoRequest request, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar presupuestos de un negocio" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _presupuestoService.UpdateEstadoAsync(id, request.Estado, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { message = ex.Message });
                }
                // Conflicto de estado
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Anula un presupuesto (solo si está Pendiente)
        /// </summary>
        /// <param name="id">ID del presupuesto a anular</param>
        /// <returns>Sin contenido</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Anular(int id, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar presupuestos de un negocio" });
            }

            try
            {
                await _presupuestoService.AnularAsync(id, ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { message = ex.Message });
                }
                // Conflicto de estado
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Convierte un presupuesto a venta
        /// </summary>
        /// <param name="id">ID del presupuesto a convertir</param>
        /// <returns>Venta creada</returns>
        [HttpPost("{id}/convertir")]
        [ProducesResponseType(typeof(VentaResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Convertir(int id, CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar presupuestos de un negocio" });
            }

            try
            {
                var result = await _presupuestoService.ConvertirAVentaAsync(id, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { message = ex.Message });
                }
                // Conflicto de estado o stock insuficiente
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
