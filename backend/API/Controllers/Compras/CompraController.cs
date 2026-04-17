using API.DTO.Request.Compras;
using API.DTO.Response.Compras;
using API.Services.Compras;
using API.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Compras
{
    [ApiController]
    [Route("api/v1/compras")]
    [Authorize]
    public class CompraController : ControllerBase
    {
        private readonly ICompraService _compraService;
        private readonly ICurrentUserService _currentUser;

        public CompraController(ICompraService compraService, ICurrentUserService currentUser)
        {
            _compraService = compraService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene todas las compras del negocio
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CompraListResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAll()
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar compras de un negocio" });
            }

            var result = await _compraService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una compra específica por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CompraResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar compras de un negocio" });
            }

            var compra = await _compraService.GetByIdAsync(id);
            if (compra == null)
            {
                return NotFound(new { message = "Compra no encontrada" });
            }

            return Ok(compra);
        }

        /// <summary>
        /// Crea una nueva compra (Admin y Gerente)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CompraResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Create([FromBody] CrearCompraRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar compras de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "Solo el administrador o gerente pueden crear compras" });
            }

            try
            {
                var result = await _compraService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Anula una compra (Admin y Gerente)
        /// </summary>
        [HttpPost("{id}/anular")]
        [ProducesResponseType(typeof(CompraResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Anular(int id, [FromBody] AnularCompraRequest? request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar compras de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "Solo el administrador o gerente pueden anular compras" });
            }

            try
            {
                var result = await _compraService.AnularAsync(id, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}