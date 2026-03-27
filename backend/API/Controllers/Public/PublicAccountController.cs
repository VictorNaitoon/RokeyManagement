using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTO.Request.Public;
using API.DTO.Response.Public;
using API.Services.Publicos;
using API.Services.Common;

namespace API.Controllers.Publicos
{
    [ApiController]
    [Route("api/v1/publico/cuenta")]
    [Authorize]
    public class CuentaPublicaController : ControllerBase
    {
        private readonly ICuentaPublicaService _cuentaService;
        private readonly ICurrentUserService _currentUser;

        public CuentaPublicaController(ICuentaPublicaService cuentaService, ICurrentUserService currentUser)
        {
            _cuentaService = cuentaService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Lista los clientes del negocio (solo el Dueño puede ver)
        /// </summary>
        [HttpGet("clientes")]
        [ProducesResponseType(typeof(List<AccountResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetClientes()
        {
            // Solo el Dueño (Admin) puede ver clientes, NO el SuperAdmin
            if (_currentUser.Rol != "Dueño" && _currentUser.Rol != "Admin" && _currentUser.Rol != "Administrador")
            {
                return StatusCode(403, new { error = "Solo el administrador puede ver los clientes" });
            }

            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { error = "Debe belongecer a un negocio" });
            }

            var clientes = await _cuentaService.GetClientesAsync(_currentUser.NegocioId);
            return Ok(clientes);
        }

        /// <summary>
        /// Crea un nuevo cliente (solo el Dueño puede crear)
        /// </summary>
        [HttpPost("clientes")]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CrearCliente([FromBody] CrearClienteRequest request)
        {
            // Solo el Dueño (Admin) puede crear clientes, NO el SuperAdmin
            if (_currentUser.Rol != "Dueño" && _currentUser.Rol != "Admin" && _currentUser.Rol != "Administrador")
            {
                return StatusCode(403, new { error = "Solo el administrador puede crear clientes" });
            }

            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { error = "Debe belongecer a un negocio" });
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest(new { error = "El nombre es requerido" });
            }

            var cliente = await _cuentaService.CrearClienteAsync(request, _currentUser.NegocioId);
            return CreatedAtAction(nameof(GetClientes), new { }, cliente);
        }

        /// <summary>
        /// Actualiza un cliente existente (solo el Dueño)
        /// </summary>
        [HttpPut("clientes/{id}")]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarCliente(int id, [FromBody] ActualizarClienteRequest request)
        {
            if (_currentUser.Rol != "Dueño" && _currentUser.Rol != "Admin" && _currentUser.Rol != "Administrador")
            {
                return StatusCode(403, new { error = "Solo el administrador puede modificar clientes" });
            }

            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { error = "Debe belongecer a un negocio" });
            }

            var cliente = await _cuentaService.ActualizarClienteAsync(id, request, _currentUser.NegocioId);
            
            if (cliente == null)
            {
                return NotFound(new { error = "Cliente no encontrado" });
            }

            return Ok(cliente);
        }

        /// <summary>
        /// Elimina (desactiva) un cliente (solo el Dueño)
        /// </summary>
        [HttpDelete("clientes/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            if (_currentUser.Rol != "Dueño" && _currentUser.Rol != "Admin" && _currentUser.Rol != "Administrador")
            {
                return StatusCode(403, new { error = "Solo el administrador puede eliminar clientes" });
            }

            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { error = "Debe belongecer a un negocio" });
            }

            var resultado = await _cuentaService.EliminarClienteAsync(id, _currentUser.NegocioId);
            
            if (!resultado)
            {
                return NotFound(new { error = "Cliente no encontrado" });
            }

            return NoContent();
        }
    }
}