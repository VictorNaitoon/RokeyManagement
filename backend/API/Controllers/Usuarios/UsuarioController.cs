using API.DTO.Request.Usuarios;
using API.DTO.Response.Usuarios;
using API.Services.Common;
using API.Services.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Usuarios
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ICurrentUserService _currentUser;

        public UsuarioController(IUsuarioService usuarioService, ICurrentUserService currentUser)
        {
            _usuarioService = usuarioService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene todos los usuarios del negocio (solo Admin)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(UsuarioListResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAll()
        {
            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede ver la lista de usuarios" });
            }

            var result = await _usuarioService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un usuario específico por ID (solo Admin)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede ver usuarios" });
            }

            var usuario = await _usuarioService.GetByIdAsync(id);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            return Ok(usuario);
        }

        /// <summary>
        /// Crea un nuevo usuario (solo Admin)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(UsuarioResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Create([FromBody] CrearUsuarioRequest request)
        {
            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede crear usuarios" });
            }

            try
            {
                var result = await _usuarioService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un usuario existente (solo Admin)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UsuarioResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarUsuarioRequest request)
        {
            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede actualizar usuarios" });
            }

            try
            {
                var result = await _usuarioService.UpdateAsync(id, request);
                if (result == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Desactiva un usuario (soft delete) (solo Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede eliminar usuarios" });
            }

            try
            {
                var result = await _usuarioService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario actual
        /// </summary>
        [HttpPost("cambiar-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest request)
        {
            try
            {
                var result = await _usuarioService.CambiarPasswordAsync(request);
                return Ok(new { message = "Contraseña actualizada exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}