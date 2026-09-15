using API.DTO.Request.Informes;
using API.DTO.Response.Informes;
using API.Services.Informes;
using API.Services.Common;
using FluentValidation;
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
        private readonly IInformesExportService _exportService;
        private readonly IValidator<ExportInformesQuery> _exportValidator;
        private readonly IValidator<InformesQuery> _informesValidator;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<InformesController> _logger;

        public InformesController(IInformesService informesService, IInformesExportService exportService, IValidator<ExportInformesQuery> exportValidator, IValidator<InformesQuery> informesValidator, ICurrentUserService currentUser, ILogger<InformesController> logger)
        {
            _informesService = informesService;
            _exportService = exportService;
            _exportValidator = exportValidator;
            _informesValidator = informesValidator;
            _currentUser = currentUser;
            _logger = logger;
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

            await _informesValidator.ValidateAndThrowAsync(new InformesQuery(preset, fechaDesde, fechaHasta, null), ct);

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
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetProductosTop(
            [FromQuery] int cantidad = 10,
            [FromQuery] string? preset = null,
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

            await _informesValidator.ValidateAndThrowAsync(new InformesQuery(preset, fechaDesde, fechaHasta, cantidad), ct);

            var result = await _informesService.GetProductosTopAsync(cantidad, fechaDesde, fechaHasta, preset, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene el flujo de caja (ingresos vs egresos)
        /// </summary>
        /// <param name="fechaDesde">Fecha inicial opcional</param>
        /// <param name="fechaHasta">Fecha final opcional</param>
        /// <param name="preset">Preset: hoy|semana|mes</param>
        /// <returns>Flujo de caja</returns>
        [HttpGet("flujo-caja")]
        [ProducesResponseType(typeof(FlujoCajaResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetFlujoCaja(
            [FromQuery] string? preset = null,
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

            await _informesValidator.ValidateAndThrowAsync(new InformesQuery(preset, fechaDesde, fechaHasta, null), ct);

            var result = await _informesService.GetFlujoCajaAsync(fechaDesde, fechaHasta, preset, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene ingresos vs gastos (Ventas - Compras)
        /// </summary>
        /// <param name="fechaDesde">Fecha inicial opcional</param>
        /// <param name="fechaHasta">Fecha final opcional</param>
        /// <param name="preset">Preset: hoy|semana|mes</param>
        /// <returns>Ingresos vs Gastos</returns>
        [HttpGet("ingresos-gastos")]
        [ProducesResponseType(typeof(IngresosGastosResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetIngresosGastos(
            [FromQuery] string? preset = null,
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

            await _informesValidator.ValidateAndThrowAsync(new InformesQuery(preset, fechaDesde, fechaHasta, null), ct);

            var result = await _informesService.GetIngresosGastosAsync(fechaDesde, fechaHasta, preset, ct);
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
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetVentasPorPago(
            [FromQuery] string? preset = null,
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

            await _informesValidator.ValidateAndThrowAsync(new InformesQuery(preset, fechaDesde, fechaHasta, null), ct);

            var result = await _informesService.GetVentasPorPagoAsync(fechaDesde, fechaHasta, preset, ct);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene las ventas discriminadas por vendedor
        /// </summary>
        /// <param name="fechaDesde">Fecha inicial opcional</param>
        /// <param name="fechaHasta">Fecha final opcional</param>
        /// <param name="preset">Preset: hoy|semana|mes</param>
        /// <returns>Ventas por vendedor</returns>
        [HttpGet("ventas-por-vendedor")]
        [ProducesResponseType(typeof(VentasPorVendedorResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetVentasPorVendedor(
            [FromQuery] string? preset = null,
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

            await _informesValidator.ValidateAndThrowAsync(new InformesQuery(preset, fechaDesde, fechaHasta, null), ct);

            var result = await _informesService.GetVentasPorVendedorAsync(fechaDesde, fechaHasta, preset, ct);
            return Ok(result);
        }

        /// <summary>
        /// Exporta un informe en CSV, PDF o DOCX
        /// </summary>
        [HttpGet("{tipo}/export")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Export(
            [FromRoute] string tipo,
            [FromQuery] string formato,
            [FromQuery] string? preset = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? cantidad = null,
            CancellationToken ct = default)
        {
            if (_currentUser.IsSuperAdmin)
                return StatusCode(403, new { message = "El super administrador no puede acceder a informes de un negocio" });
            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
                return StatusCode(403, new { message = "Solo los administradores y gerentes pueden acceder a informes" });

            var query = new ExportInformesQuery(tipo, formato, preset, fechaDesde, fechaHasta, cantidad);
            await _exportValidator.ValidateAndThrowAsync(query, ct);

            try
            {
                var result = await _exportService.ExportAsync(query, ct);
                // Use FileContentResult with byte[] — no MemoryStream lifecycle, no ObjectDisposedException.
                // enableRangeProcessing:false avoids range-header handling that can confuse PDF viewers.
                return File(result.Bytes, result.ContentType, result.FileName, enableRangeProcessing: false);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export endpoint failed tipo={Tipo} formato={Formato} preset={Preset}", tipo, formato, preset);
                throw;
            }
        }
    }
}
