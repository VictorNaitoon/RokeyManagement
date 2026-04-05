using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTO.Request.Clientes;
using API.DTO.Response.Clientes;
using API.Services.Clientes;
using API.Services.Common;

namespace API.Controllers.Clientes
{
    [ApiController]
    [Route("api/v1/clientes")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly ICurrentUserService _currentUser;

        public ClientesController(IClienteService clienteService, ICurrentUserService currentUser)
        {
            _clienteService = clienteService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Lista los clientes del negocio (Admin y Gerente solamente)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AccountResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll([FromQuery] bool incluirConsumidorFinal = false)
        {
            // Solo Admin y Gerente pueden listar todos los clientes
            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { error = "No tiene permisos para listar clientes" });
            }

            // SuperAdmin check - si NegocioId es 0 o negativo, es SuperAdmin
            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { error = "Debe pertenecer a un negocio para ver clientes" });
            }

            var clientes = await _clienteService.GetAllAsync(_currentUser.NegocioId, incluirConsumidorFinal);
            return Ok(clientes);
        }

        /// <summary>
        /// Obtiene un cliente por ID (Admin, Gerente y Vendedor)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            // Admin, Gerente y Vendedor pueden ver un cliente por ID
            if (!_currentUser.IsAdmin && !_currentUser.IsManager && !_currentUser.IsVendedor)
            {
                return StatusCode(403, new { error = "No tiene permisos para ver clientes" });
            }

            // SuperAdmin check
            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { error = "Debe pertenecer a un negocio para ver clientes" });
            }

            var cliente = await _clienteService.GetByIdAsync(id, _currentUser.NegocioId);
            
            if (cliente == null)
            {
                return NotFound(new { error = "Cliente no encontrado" });
            }

            return Ok(cliente);
        }

        /// <summary>
        /// Crea un nuevo cliente (Admin, Gerente y Vendedor)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CrearClienteRequest request)
        {
            // Admin, Gerente y Vendedor pueden crear clientes
            if (!_currentUser.IsAdmin && !_currentUser.IsManager && !_currentUser.IsVendedor)
            {
                return StatusCode(403, new { error = "No tiene permisos para crear clientes" });
            }

            // SuperAdmin check
            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { error = "Debe pertenecer a un negocio para crear clientes" });
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest(new { error = "El nombre es requerido" });
            }

            var cliente = await _clienteService.CreateAsync(request, _currentUser.NegocioId);
            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
        }

        /// <summary>
        /// Actualiza un cliente existente (Admin y Gerente solamente)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarClienteRequest request)
        {
            // Solo Admin y Gerente pueden modificar
            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { error = "No tiene permisos para modificar clientes" });
            }

            // SuperAdmin check
            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { error = "Debe pertenecer a un negocio para modificar clientes" });
            }

            var cliente = await _clienteService.UpdateAsync(id, request, _currentUser.NegocioId);
            
            if (cliente == null)
            {
                return NotFound(new { error = "Cliente no encontrado" });
            }

            return Ok(cliente);
        }

        /// <summary>
        /// Elimina (soft delete) un cliente (Admin y Gerente solamente)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            // Solo Admin y Gerente pueden eliminar
            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { error = "No tiene permisos para eliminar clientes" });
            }

            // SuperAdmin check
            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { error = "Debe pertenecer a un negocio para eliminar clientes" });
            }

            var resultado = await _clienteService.DeleteAsync(id, _currentUser.NegocioId);
            
            if (!resultado)
            {
                return NotFound(new { error = "Cliente no encontrado" });
            }

            return NoContent();
        }

        /// <summary>
        /// Obtiene las ventas de un cliente (paginado) - Phase 3
        /// </summary>
        [HttpGet("{id}/ventas")]
        [ProducesResponseType(typeof(VentaClienteListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVentas(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Admin, Gerente y Vendedor pueden ver ventas de clientes
            if (!_currentUser.IsAdmin && !_currentUser.IsManager && !_currentUser.IsVendedor)
            {
                return StatusCode(403, new { error = "No tiene permisos para ver ventas de clientes" });
            }

            // SuperAdmin check
            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { error = "Debe pertenecer a un negocio" });
            }

            // Verificar que el cliente existe
            var cliente = await _clienteService.GetByIdAsync(id, _currentUser.NegocioId);
            if (cliente == null)
            {
                return NotFound(new { error = "Cliente no encontrado" });
            }

            var ventas = await _clienteService.GetVentasAsync(id, _currentUser.NegocioId, page, pageSize);
            return Ok(ventas);
        }

        /// <summary>
        /// Obtiene el saldo pendiente de un cliente - Phase 3
        /// </summary>
        [HttpGet("{id}/saldo")]
        [ProducesResponseType(typeof(SaldoClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSaldo(int id)
        {
            // Admin, Gerente y Vendedor pueden ver saldo de clientes
            if (!_currentUser.IsAdmin && !_currentUser.IsManager && !_currentUser.IsVendedor)
            {
                return StatusCode(403, new { error = "No tiene permisos para ver saldo de clientes" });
            }

            // SuperAdmin check
            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { error = "Debe pertenecer a un negocio" });
            }

            // Verificar que el cliente existe
            var cliente = await _clienteService.GetByIdAsync(id, _currentUser.NegocioId);
            if (cliente == null)
            {
                return NotFound(new { error = "Cliente no encontrado" });
            }

            var saldo = await _clienteService.GetSaldoAsync(id, _currentUser.NegocioId);
            return Ok(saldo);
        }

        /// <summary>
        /// Obtiene los pagos de un cliente - Phase 3
        /// </summary>
        [HttpGet("{id}/pagos")]
        [ProducesResponseType(typeof(List<PagoClienteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPagos(int id)
        {
            // Admin, Gerente y Vendedor pueden ver pagos de clientes
            if (!_currentUser.IsAdmin && !_currentUser.IsManager && !_currentUser.IsVendedor)
            {
                return StatusCode(403, new { error = "No tiene permisos para ver pagos de clientes" });
            }

            // SuperAdmin check
            if (_currentUser.NegocioId <= 0)
            {
                return StatusCode(403, new { error = "Debe pertenecer a un negocio" });
            }

            // Verificar que el cliente existe
            var cliente = await _clienteService.GetByIdAsync(id, _currentUser.NegocioId);
            if (cliente == null)
            {
                return NotFound(new { error = "Cliente no encontrado" });
            }

            var pagos = await _clienteService.GetPagosAsync(id, _currentUser.NegocioId);
            return Ok(pagos);
        }
    }
}
