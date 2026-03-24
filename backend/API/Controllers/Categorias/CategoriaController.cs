using API.DTO.Request.Categorias;
using API.DTO.Response.Categorias;
using API.Services.Categorias;
using API.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Categorias
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;
        private readonly ICurrentUserService _currentUser;

        public CategoriaController(ICategoriaService categoriaService, ICurrentUserService currentUser)
        {
            _categoriaService = categoriaService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene todas las categorías del negocio (solo Admin)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CategoriaListResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAll()
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar categorías de un negocio" });
            }

            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede ver la lista de categorías" });
            }

            var result = await _categoriaService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una categoría específica por ID (solo Admin)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoriaResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar categorías de un negocio" });
            }

            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede ver categorías" });
            }

            var categoria = await _categoriaService.GetByIdAsync(id);
            if (categoria == null)
            {
                return NotFound(new { message = "Categoría no encontrada" });
            }

            return Ok(categoria);
        }

        /// <summary>
        /// Crea una nueva categoría (solo Admin)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CategoriaResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Create([FromBody] CrearCategoriaRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar categorías de un negocio" });
            }

            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede crear categorías" });
            }

            try
            {
                var result = await _categoriaService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una categoría existente (solo Admin)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CategoriaResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarCategoriaRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar categorías de un negocio" });
            }

            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede actualizar categorías" });
            }

            try
            {
                var result = await _categoriaService.UpdateAsync(id, request);
                if (result == null)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Desactiva una categoría (soft delete) (solo Admin)
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
                return StatusCode(403, new { message = "El super administrador no puede gestionar categorías de un negocio" });
            }

            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede eliminar categorías" });
            }

            try
            {
                var result = await _categoriaService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
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