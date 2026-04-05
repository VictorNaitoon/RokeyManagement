using API.DTO.Request.Auditoria;
using API.DTO.Response.Auditoria;
using API.Services.Auditoria;
using API.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Auditoria
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin,Administrador,Dueño,Manager,Gerente,SuperAdmin")]
    public class AuditoriaController : ControllerBase
    {
        private readonly IAuditoriaService _auditoriaService;
        private readonly ICurrentUserService _currentUser;

        public AuditoriaController(IAuditoriaService auditoriaService, ICurrentUserService currentUser)
        {
            _auditoriaService = auditoriaService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Lista registros de auditoría con filtros y paginación
        /// </summary>
        /// <param name="entidad">Filtrar por entidad (Venta, Compra, Producto, Presupuesto, Usuario, Caja)</param>
        /// <param name="idUsuario">Filtrar por ID de usuario</param>
        /// <param name="fechaDesde">Fecha desde (inclusive)</param>
        /// <param name="fechaHasta">Fecha hasta (inclusive)</param>
        /// <param name="idRegistro">Filtrar por ID del registro</param>
        /// <param name="page">Número de página (default: 1)</param>
        /// <param name="pageSize">Tamaño de página (default: 20, max: 100)</param>
        /// <returns>Lista paginada de registros de auditoría</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? entidad = null,
            [FromQuery] int? idUsuario = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? idRegistro = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede acceder a auditoría de un negocio" });
            }

            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { message = "Debe pertenecer a un negocio" });
            }

            var filtro = new FiltroAuditoriaRequest
            {
                Entidad = entidad,
                IdUsuario = idUsuario,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                IdRegistro = idRegistro,
                Page = page,
                PageSize = Math.Min(pageSize, 100)
            };

            var (items, total) = await _auditoriaService.ListarAsync(filtro);

            return Ok(new
            {
                items,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)filtro.PageSize)
            });
        }

        /// <summary>
        /// Obtiene un registro de auditoría específico por ID
        /// </summary>
        /// <param name="id">ID del registro de auditoría</param>
        /// <returns>Datos completos del registro de auditoría incluyendo JSON diff</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AuditoriaResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede acceder a auditoría de un negocio" });
            }

            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { message = "Debe pertenecer a un negocio" });
            }

            var auditoria = await _auditoriaService.ObtenerPorIdAsync(id);

            if (auditoria == null)
            {
                return NotFound(new { message = "Registro de auditoría no encontrado" });
            }

            return Ok(auditoria);
        }
    }
}