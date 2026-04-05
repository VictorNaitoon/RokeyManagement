using API.DTO.Request.Negocios;
using API.DTO.Response.Negocios;
using API.Services.Common;
using API.Services.Negocios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Negocios
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class NegocioController : ControllerBase
    {
        private readonly INegocioService _negocioService;
        private readonly ICurrentUserService _currentUser;

        public NegocioController(INegocioService negocioService, ICurrentUserService currentUser)
        {
            _negocioService = negocioService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene los datos del negocio al que pertenece el usuario actual
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(NegocioResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMiNegocio()
        {
            // El SuperAdmin no tiene negocio propio
            if (_currentUser.Rol == "SuperAdmin")
            {
                return StatusCode(403, new { message = "El Super Admin no tiene un negocio asociado" });
            }

            var negocio = await _negocioService.GetMiNegocioAsync();
            if (negocio == null)
            {
                return NotFound(new { message = "Negocio no encontrado" });
            }

            return Ok(negocio);
        }

        /// <summary>
        /// Actualiza los datos del negocio (solo el Administrador/Dueño)
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(NegocioResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateMiNegocio([FromBody] ActualizarNegocioRequest request)
        {
            try
            {
                var negocio = await _negocioService.UpdateMiNegocioAsync(request);
                if (negocio == null)
                {
                    return NotFound(new { message = "Negocio no encontrado" });
                }

                return Ok(negocio);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}