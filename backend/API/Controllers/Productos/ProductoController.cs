using API.DTO.Request.Productos;
using API.DTO.Response.Productos;
using API.Services.Productos;
using API.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Productos
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoService _productoService;
        private readonly ICurrentUserService _currentUser;

        public ProductoController(IProductoService productoService, ICurrentUserService currentUser)
        {
            _productoService = productoService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene todos los productos del negocio actual con búsqueda opcional
        /// </summary>
        /// <param name="busqueda">Término de búsqueda en nombre o código (opcional)</param>
        /// <returns>Lista de productos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ProductoListResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAll([FromQuery] string? busqueda = null)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar productos de un negocio" });
            }

            var result = await _productoService.GetAllAsync(busqueda);
            return Ok(result);
        }

        /// <summary>
        /// Lista productos con stock bajo (Admin, Gerente, Vendedor)
        /// </summary>
        [HttpGet("alertas")]
        [ProducesResponseType(typeof(List<ProductoAlertaResponse>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAlertas()
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar productos de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager && !_currentUser.IsVendedor)
                return StatusCode(403, new { message = "No tiene permisos para ver alertas de stock" });

            if (_currentUser.NegocioId <= 0)
                return StatusCode(403, new { message = "Debe pertenecer a un negocio" });

            var alertas = await _productoService.GetProductosConStockBajoAsync(_currentUser.NegocioId);
            return Ok(alertas);
        }

        /// <summary>
        /// Cuenta productos con stock bajo (Admin, Gerente, Vendedor)
        /// </summary>
        [HttpGet("alertas/contador")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetContadorAlertas()
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar productos de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager && !_currentUser.IsVendedor)
                return StatusCode(403, new { message = "No tiene permisos para ver alertas de stock" });

            if (_currentUser.NegocioId <= 0)
                return StatusCode(403, new { message = "Debe pertenecer a un negocio" });

            var cantidad = await _productoService.GetContadorStockBajoAsync(_currentUser.NegocioId);
            return Ok(new { cantidad });
        }

        /// <summary>
        /// Obtiene un producto específico por ID
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Datos del producto</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductoResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar productos de un negocio" });
            }

            var producto = await _productoService.GetByIdAsync(id);
            
            if (producto == null)
            {
                return NotFound(new { message = "Producto no encontrado" });
            }

            return Ok(producto);
        }

        /// <summary>
        /// Crea un nuevo producto
        /// </summary>
        /// <param name="request">Datos del producto a crear</param>
        /// <returns>Producto creado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProductoResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Create([FromBody] CrearProductoRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar productos de un negocio" });
            }

            try
            {
                var result = await _productoService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        /// <param name="id">ID del producto a actualizar</param>
        /// <param name="request">Datos actualizados del producto</param>
        /// <returns>Producto actualizado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductoResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarProductoRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar productos de un negocio" });
            }

            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede actualizar productos" });
            }

            try
            {
                var result = await _productoService.UpdateAsync(id, request);
                
                if (result == null)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina (soft delete) un producto existente
        /// </summary>
        /// <param name="id">ID del producto a eliminar</param>
        /// <returns>Sin contenido</returns>
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
                return StatusCode(403, new { message = "El super administrador no puede gestionar productos de un negocio" });
            }

            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede eliminar productos" });
            }

            try
            {
                var result = await _productoService.DeleteAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Reactiva un producto desactivado
        /// </summary>
        /// <param name="id">ID del producto a activar</param>
        /// <returns>Sin contenido</returns>
        [HttpPost("{id}/activar")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Activar(int id)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar productos de un negocio" });
            }

            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new { message = "Solo el administrador puede activar productos" });
            }

            try
            {
                var result = await _productoService.ActivarAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                return Ok(new { message = "Producto activado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Duplica un producto existente
        /// </summary>
        /// <param name="id">ID del producto a duplicar</param>
        /// <param name="nuevoNombre">Nombre para el producto duplicado</param>
        /// <returns>Producto duplicado</returns>
        [HttpPost("{id}/duplicar")]
        [ProducesResponseType(typeof(ProductoResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Duplicate(int id, [FromBody] string nuevoNombre)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar productos de un negocio" });
            }

            if (string.IsNullOrWhiteSpace(nuevoNombre))
            {
                return BadRequest(new { message = "El nombre del producto duplicado no puede estar vacío" });
            }

            try
            {
                var result = await _productoService.DuplicateAsync(id, nuevoNombre.Trim());
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Historial de movimientos de stock de un producto (Solo Admin)
        /// </summary>
        [HttpGet("{id}/movimientos")]
        [ProducesResponseType(typeof(List<MovimientoStockResponse>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMovimientos(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede gestionar productos de un negocio" });
            }

            if (!_currentUser.IsAdmin)
                return StatusCode(403, new { message = "Solo el administrador puede ver auditoría de stock" });

            if (_currentUser.NegocioId <= 0)
                return StatusCode(403, new { message = "Debe pertenecer a un negocio" });

            // Verify product exists
            var producto = await _productoService.GetByIdAsync(id);
            if (producto == null)
                return NotFound(new { message = "Producto no encontrado" });

            var movimientos = await _productoService.GetMovimientosStockAsync(id, _currentUser.NegocioId, page, pageSize);
            return Ok(movimientos);
        }

        /// <summary>
        /// Actualización masiva de precios de productos por categoría (Admin, Administrador, Dueño, Gerente)
        /// </summary>
        [HttpPost("actualizar-precios/categoria")]
        [ProducesResponseType(typeof(ActualizacionMasivaPreciosResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ActualizarPreciosPorCategoria([FromBody] ActualizacionMasivaPreciosCategoriaRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El SuperAdmin no puede actualizar precios de productos. Esta operación es exclusiva del negocio." });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "No tiene permisos para actualizar precios masivamente. Solo los roles Admin, Administrador, Dueño y Gerente pueden realizar esta acción." });
            }

            try
            {
                var result = await _productoService.ActualizarPreciosPorCategoriaAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualización masiva de precios de productos específicos (Admin, Administrador, Dueño, Gerente)
        /// </summary>
        [HttpPost("actualizar-precios/productos")]
        [ProducesResponseType(typeof(ActualizacionMasivaPreciosResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ActualizarPreciosPorProductos([FromBody] ActualizacionMasivaPreciosProductosRequest request)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El SuperAdmin no puede actualizar precios de productos. Esta operación es exclusiva del negocio." });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "No tiene permisos para actualizar precios masivamente. Solo los roles Admin, Administrador, Dueño y Gerente pueden realizar esta acción." });
            }

            try
            {
                var result = await _productoService.ActualizarPreciosPorProductosAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}