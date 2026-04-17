using API.DTO.Request.Proveedores;
using API.DTO.Response.Proveedores;
using API.Services.Proveedores;
using API.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Proveedores
{
    [ApiController]
    [Route("api/v1/proveedores")]
    [Authorize]
    public class ProveedorController : ControllerBase
    {
        private readonly IProveedorService _proveedorService;
        private readonly ICurrentUserService _currentUser;

        public ProveedorController(IProveedorService proveedorService, ICurrentUserService currentUser)
        {
            _proveedorService = proveedorService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene todos los proveedores del negocio
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProveedorListResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAll()
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar proveedores de un negocio" });
            }

            var result = await _proveedorService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un proveedor específico por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProveedorResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar proveedores de un negocio" });
            }

            var proveedor = await _proveedorService.GetByIdAsync(id);
            if (proveedor == null)
            {
                return NotFound(new { message = "Proveedor no encontrado" });
            }

            return Ok(proveedor);
        }

        /// <summary>
        /// Crea un nuevo proveedor (solo Admin)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ProveedorResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Create([FromBody] CrearProveedorRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar proveedores de un negocio" });
            }

            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede crear proveedores" });
            }

            try
            {
                var result = await _proveedorService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un proveedor existente (solo Admin)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProveedorResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarProveedorRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar proveedores de un negocio" });
            }

            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede actualizar proveedores" });
            }

            try
            {
                var result = await _proveedorService.UpdateAsync(id, request);
                if (result == null)
                {
                    return NotFound(new { message = "Proveedor no encontrado" });
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un proveedor (solo Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar proveedores de un negocio" });
            }

            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede eliminar proveedores" });
            }

            try
            {
                var result = await _proveedorService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Proveedor no encontrado" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}