using API.DTO.Response.Informes;
using API.Services.Informes;
using API.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Informes
{
    [ApiController]
    [Route("api/v1/informes")]
    [Authorize(Roles = "Dueño,Gerente")]
    public class InformesController : ControllerBase
    {
        private readonly IInformesService _informesService;
        private readonly ICurrentUserService _currentUser;

        public InformesController(IInformesService informesService, ICurrentUserService currentUser)
        {
            _informesService = informesService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene el resumen de ventas (total, cantidad, ticket promedio)
        /// </summary>
        /// <param name="fechaDesde">Fecha inicial opcional (formato: YYYY-MM-DD)</param>
        /// <param name="fechaHasta">Fecha final opcional (formato: YYYY-MM-DD)</param>
        /// <param name="preset">Preset de fecha: "hoy", "semana", "mes" (default: "mes")</param>
        /// <returns>Resumen de ventas</returns>
        [HttpGet("ventas-resumen")]
        [ProducesResponseType(typeof(VentasResumenResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetVentasResumen(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string? preset = null,
            CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede acceder a informes de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "Solo los administradores y gerentes pueden acceder a informes" });
            }

            var result = await _informesService.GetVentasResumenAsync(fechaDesde, fechaHasta, preset, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los productos más vendidos por ingresos/cantidad
        /// </summary>
        /// <param name="cantidad">Cantidad de productos a retornar (default: 10, max: 50)</param>
        /// <param name="fechaDesde">Fecha inicial opcional</param>
        /// <param name="fechaHasta">Fecha final opcional</param>
        /// <returns>Lista de productos más vendidos</returns>
        [HttpGet("productos-top")]
        [ProducesResponseType(typeof(ProductosTopResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetProductosTop(
            [FromQuery] int cantidad = 10,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede acceder a informes de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "Solo los administradores y gerentes pueden acceder a informes" });
            }

            var result = await _informesService.GetProductosTopAsync(cantidad, fechaDesde, fechaHasta, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene el flujo de caja (ingresos vs egresos)
        /// </summary>
        /// <param name="fechaDesde">Fecha inicial opcional</param>
        /// <param name="fechaHasta">Fecha final opcional</param>
        /// <returns>Flujo de caja</returns>
        [HttpGet("flujo-caja")]
        [ProducesResponseType(typeof(FlujoCajaResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetFlujoCaja(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede acceder a informes de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "Solo los administradores y gerentes pueden acceder a informes" });
            }

            var result = await _informesService.GetFlujoCajaAsync(fechaDesde, fechaHasta, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene ingresos vs gastos (Ventas - Compras)
        /// </summary>
        /// <param name="fechaDesde">Fecha inicial opcional</param>
        /// <param name="fechaHasta">Fecha final opcional</param>
        /// <returns>Ingresos vs Gastos</returns>
        [HttpGet("ingresos-gastos")]
        [ProducesResponseType(typeof(IngresosGastosResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetIngresosGastos(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede acceder a informes de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "Solo los administradores y gerentes pueden acceder a informes" });
            }

            var result = await _informesService.GetIngresosGastosAsync(fechaDesde, fechaHasta, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los productos con stock bajo mínimo (solo productos, no servicios)
        /// </summary>
        /// <returns>Lista de productos con alerta de stock</returns>
        [HttpGet("alertas-stock")]
        [ProducesResponseType(typeof(AlertasStockResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAlertasStock(CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede acceder a informes de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "Solo los administradores y gerentes pueden acceder a informes" });
            }

            var result = await _informesService.GetAlertasStockAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene las ventas discriminadas por método de pago
        /// </summary>
        /// <param name="fechaDesde">Fecha inicial opcional</param>
        /// <param name="fechaHasta">Fecha final opcional</param>
        /// <returns>Ventas por método de pago</returns>
        [HttpGet("ventas-por-pago")]
        [ProducesResponseType(typeof(VentasPorPagoResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetVentasPorPago(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede acceder a informes de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "Solo los administradores y gerentes pueden acceder a informes" });
            }

            var result = await _informesService.GetVentasPorPagoAsync(fechaDesde, fechaHasta, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene las ventas discriminadas por vendedor
        /// </summary>
        /// <param name="fechaDesde">Fecha inicial opcional</param>
        /// <param name="fechaHasta">Fecha final opcional</param>
        /// <returns>Ventas por vendedor</returns>
        [HttpGet("ventas-por-vendedor")]
        [ProducesResponseType(typeof(VentasPorVendedorResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetVentasPorVendedor(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
            {
                return StatusCode(403, new { message = "El super administrador no puede acceder a informes de un negocio" });
            }

            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
            {
                return StatusCode(403, new { message = "Solo los administradores y gerentes pueden acceder a informes" });
            }

            var result = await _informesService.GetVentasPorVendedorAsync(fechaDesde, fechaHasta, ct);
            return Ok(result);
        }
    }
}
