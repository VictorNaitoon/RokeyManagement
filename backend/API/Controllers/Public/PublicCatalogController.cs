using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Services.Publicos;

namespace API.Controllers.Publicos
{
    [ApiController]
    [Route("api/v1/publico/catalogo")]
    [AllowAnonymous]
    public class CatalogoPublicoController : ControllerBase
    {
        private readonly ICatalogoPublicoService _catalogoService;

        public CatalogoPublicoController(ICatalogoPublicoService catalogoService)
        {
            _catalogoService = catalogoService;
        }

        /// <summary>
        /// Obtiene el catálogo completo de productos visibles para un negocio (público, sin autenticación)
        /// </summary>
        /// <param name="idNegocio">ID del negocio (desde header X-Negocio-Id o parámetro)</param>
        [HttpGet]
        [ProducesResponseType(typeof(DTO.Response.Public.ProductoPublicResponse[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCatalogo([FromQuery] int idNegocio)
        {
            if (idNegocio <= 0)
            {
                return BadRequest(new { error = "Se requiere el ID del negocio" });
            }

            var productos = await _catalogoService.GetCatalogoAsync(idNegocio);
            
            return Ok(productos);
        }

        /// <summary>
        /// Obtiene un producto específico del catálogo público
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <param name="idNegocio">ID del negocio</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DTO.Response.Public.ProductoPublicResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProducto(int id, [FromQuery] int idNegocio)
        {
            if (idNegocio <= 0)
            {
                return BadRequest(new { error = "Se requiere el ID del negocio" });
            }

            var producto = await _catalogoService.GetProductoAsync(idNegocio, id);
            
            if (producto == null)
            {
                return NotFound(new { error = "Producto no encontrado" });
            }

            return Ok(producto);
        }
    }
}