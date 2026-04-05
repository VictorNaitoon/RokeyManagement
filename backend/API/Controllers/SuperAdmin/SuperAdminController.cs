using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTO.Request;
using API.DTO.Request.SuperAdmin;
using API.DTO.Request.Suscripcion;
using API.DTO.Response;
using API.DTO.Response.SuperAdmin;
using API.DTO.Response.Suscripcion;
using API.Models;
using API.Services.SuperAdmin;
using API.Services.Suscripcion;
using PlanResponse = API.DTO.Response.SuperAdmin.PlanResponse;

namespace API.Controllers.SuperAdmin
{
    [ApiController]
    [Route("api/v1/super-admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly ISuperAdminService _superAdminService;
        private readonly ISuscripcionService _suscripcionService;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(
            ISuperAdminService superAdminService,
            ISuscripcionService suscripcionService,
            ILogger<SuperAdminController> logger)
        {
            _superAdminService = superAdminService;
            _suscripcionService = suscripcionService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint público de prueba - NO requiere token
        /// </summary>
        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping()
        {
            return Ok(new { mensaje = "Super Admin endpoint funcionando!", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Obtener métricas del dashboard
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(DashboardMetrics), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard()
        {
            var metrics = await _superAdminService.GetDashboardMetricsAsync();
            return Ok(metrics);
        }

        /// <summary>
        /// Obtener métricas de la plataforma
        /// </summary>
        [HttpGet("metrics")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(PlatformMetrics), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMetrics()
        {
            var metrics = await _superAdminService.GetPlatformMetricsAsync();
            return Ok(metrics);
        }

        /// <summary>
        /// Listar todos los tenants (negocios)
        /// </summary>
        [HttpGet("tenants")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(List<TenantResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenants()
        {
            var tenants = await _superAdminService.GetAllTenantsAsync();
            
            var response = tenants.Select(n =>
            {
                var suscripcion = n.Suscripciones?.FirstOrDefault();
                var plan = suscripcion?.Plan;
                
                return new TenantResponse
                {
                    Id = n.Id,
                    Nombre = n.Nombre,
                    CUIT = n.CUIT,
                    Direccion = n.Direccion,
                    Telefono = n.Telefono,
                    Estado = n.Estado.ToString(),
                    Tipo = n.Tipo.ToString(),
                    FechaInicio = n.FechaInicioActividades,
                    TotalUsuarios = n.Usuarios?.Count ?? 0,
                    TotalProductos = n.Productos?.Count ?? 0,
                    Suscripcion = suscripcion != null ? new SuscripcionInfo
                    {
                        Id = suscripcion.Id,
                        Plan = plan?.Nombre ?? "Sin plan",
                        Estado = suscripcion.Estado.ToString(),
                        TipoFacturacion = suscripcion.TipoFacturacion.ToString(),
                        Monto = suscripcion.TipoFacturacion == Models.Enums.TipoFacturacion.Anual
                            ? plan?.PrecioAnual ?? 0
                            : plan?.PrecioMensual ?? 0,
                        FechaProximoPago = suscripcion.FechaProximoPago,
                        FechaInicio = suscripcion.FechaInicio
                    } : null
                };
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Registrar un nuevo negocio manualmente (solo Super Admin)
        /// </summary>
        [HttpPost("tenants")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
        {
            try
            {
                // Convertir tipo de negocio
                if (!Enum.TryParse<API.Models.Enums.TipoNegocio>(request.Tipo.ToString(), out var tipoEnum))
                {
                    tipoEnum = API.Models.Enums.TipoNegocio.Ferreteria;
                }

                var negocio = await _superAdminService.CreateTenantAsync(
                    emailAdmin: request.EmailAdmin,
                    passwordAdmin: request.PasswordAdmin,
                    nombreAdmin: request.NombreAdmin,
                    apellidoAdmin: request.ApellidoAdmin,
                    nombre: request.Nombre,
                    cuit: request.CUIT,
                    direccion: request.Direccion,
                    logoUrl: request.LogoURL,
                    telefono: request.Telefono,
                    email: request.Email,
                    puntoVenta: request.PuntoDeVenta,
                    condicionVentas: request.CondicionVentas,
                    tipo: tipoEnum,
                    activo: request.Activo,
                    idPlan: request.IdPlan,
                    tipoFacturacion: request.TipoFacturacion,
                    activarSuscripcion: request.ActivarSuscripcion);

                if (negocio == null)
                {
                    return BadRequest(new ErrorResponse { Error = "Error al crear el negocio" });
                }

                // Obtener datos completos para la respuesta
                var negocioCompleto = await _superAdminService.GetTenantByIdAsync(negocio.Id);
                var suscripcion = negocioCompleto?.Suscripciones?.FirstOrDefault();
                var plan = suscripcion?.Plan;

                var response = new TenantResponse
                {
                    Id = negocio.Id,
                    Nombre = negocio.Nombre,
                    CUIT = negocio.CUIT,
                    Direccion = negocio.Direccion,
                    Telefono = negocio.Telefono,
                    Estado = negocio.Estado.ToString(),
                    Tipo = negocio.Tipo.ToString(),
                    FechaInicio = negocio.FechaInicioActividades,
                    TotalUsuarios = negocioCompleto?.Usuarios?.Count ?? 0,
                    TotalProductos = negocioCompleto?.Productos?.Count ?? 0,
                    Suscripcion = suscripcion != null ? new SuscripcionInfo
                    {
                        Id = suscripcion.Id,
                        Plan = plan?.Nombre ?? "Sin plan",
                        Estado = suscripcion.Estado.ToString(),
                        TipoFacturacion = suscripcion.TipoFacturacion.ToString(),
                        Monto = suscripcion.TipoFacturacion == Models.Enums.TipoFacturacion.Anual
                            ? plan?.PrecioAnual ?? 0
                            : plan?.PrecioMensual ?? 0,
                        FechaProximoPago = suscripcion.FechaProximoPago,
                        FechaInicio = suscripcion.FechaInicio
                    } : null
                };

                return CreatedAtAction(nameof(GetTenant), new { id = negocio.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener un tenant específico
        /// </summary>
        [HttpGet("tenants/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTenant(int id)
        {
            var tenant = await _superAdminService.GetTenantByIdAsync(id);
            if (tenant == null)
            {
                return NotFound(new ErrorResponse { Error = "Tenant no encontrado" });
            }

            var suscripcion = tenant.Suscripciones?.FirstOrDefault();
            var plan = suscripcion?.Plan;

            var response = new TenantResponse
            {
                Id = tenant.Id,
                Nombre = tenant.Nombre,
                CUIT = tenant.CUIT,
                Direccion = tenant.Direccion,
                Telefono = tenant.Telefono,
                Estado = tenant.Estado.ToString(),
                Tipo = tenant.Tipo.ToString(),
                FechaInicio = tenant.FechaInicioActividades,
                TotalUsuarios = tenant.Usuarios?.Count ?? 0,
                TotalProductos = tenant.Productos?.Count ?? 0,
                Suscripcion = suscripcion != null ? new SuscripcionInfo
                {
                    Id = suscripcion.Id,
                    Plan = plan?.Nombre ?? "Sin plan",
                    Estado = suscripcion.Estado.ToString(),
                    TipoFacturacion = suscripcion.TipoFacturacion.ToString(),
                    Monto = suscripcion.TipoFacturacion == Models.Enums.TipoFacturacion.Anual
                        ? plan?.PrecioAnual ?? 0
                        : plan?.PrecioMensual ?? 0,
                    FechaProximoPago = suscripcion.FechaProximoPago,
                    FechaInicio = suscripcion.FechaInicio
                } : null
            };

            return Ok(response);
        }

        /// <summary>
        /// Actualizar estado de un tenant (Activo/Inactivo/Suspendido)
        /// </summary>
        [HttpPut("tenants/{id}/estado")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTenantEstado(int id, [FromBody] UpdateTenantEstadoRequest request)
        {
            if (!Enum.TryParse<API.Models.Enums.EstadoNegocio>(request.Estado, true, out var estado))
            {
                return BadRequest(new ErrorResponse { Error = "Estado inválido. Use: Activo, Inactivo" });
            }

            var tenant = await _superAdminService.UpdateTenantEstadoAsync(id, estado);
            if (tenant == null)
            {
                return NotFound(new ErrorResponse { Error = "Tenant no encontrado" });
            }

            return Ok(new TenantResponse
            {
                Id = tenant.Id,
                Nombre = tenant.Nombre,
                Estado = tenant.Estado.ToString()
            });
        }

        /// <summary>
        /// Listar todos los planes
        /// </summary>
        [HttpGet("planes")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(List<PlanResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlanes()
        {
            var planes = await _superAdminService.GetAllPlanesAsync();
            
            var response = planes.Select(p => new PlanResponse
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PrecioMensual = p.PrecioMensual,
                PrecioAnual = p.PrecioAnual,
                MaxUsuarios = p.MaxUsuarios,
                MaxProductos = p.MaxProductos,
                MaxTransaccionesMes = p.MaxTransaccionesMes,
                SoportePrioritario = p.SoportePrioritario,
                MultiSucursal = p.MultiSucursal,
                APIAccess = p.APIAccess,
                Activo = p.Activo,
                Orden = p.Orden
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Crear un nuevo plan
        /// </summary>
        [HttpPost("planes")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(PlanResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePlan([FromBody] PlanRequest request)
        {
            var plan = new API.Models.Plan
            {
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                PrecioMensual = request.PrecioMensual,
                PrecioAnual = request.PrecioAnual,
                MaxUsuarios = request.MaxUsuarios,
                MaxProductos = request.MaxProductos,
                MaxTransaccionesMes = request.MaxTransaccionesMes,
                SoportePrioritario = request.SoportePrioritario,
                MultiSucursal = request.MultiSucursal,
                APIAccess = request.APIAccess,
                Activo = request.Activo,
                Orden = request.Orden
            };

            var planCreado = await _superAdminService.CreatePlanAsync(plan);
            
            return CreatedAtAction(nameof(GetPlanes), new PlanResponse
            {
                Id = planCreado.Id,
                Nombre = planCreado.Nombre
            });
        }

        /// <summary>
        /// Actualizar un plan
        /// </summary>
        [HttpPut("planes/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(PlanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePlan(int id, [FromBody] PlanRequest request)
        {
            var planActualizado = new API.Models.Plan
            {
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                PrecioMensual = request.PrecioMensual,
                PrecioAnual = request.PrecioAnual,
                MaxUsuarios = request.MaxUsuarios,
                MaxProductos = request.MaxProductos,
                MaxTransaccionesMes = request.MaxTransaccionesMes,
                SoportePrioritario = request.SoportePrioritario,
                MultiSucursal = request.MultiSucursal,
                APIAccess = request.APIAccess,
                Activo = request.Activo,
                Orden = request.Orden
            };

            var plan = await _superAdminService.UpdatePlanAsync(id, planActualizado);
            if (plan == null)
            {
                return NotFound(new ErrorResponse { Error = "Plan no encontrado" });
            }

            return Ok(new PlanResponse
            {
                Id = plan.Id,
                Nombre = plan.Nombre
            });
        }

        /// <summary>
        /// Eliminar un plan (o desactivarlo si tiene suscripciones)
        /// </summary>
        [HttpDelete("planes/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var result = await _superAdminService.DeletePlanAsync(id);
            if (!result)
            {
                return NotFound(new ErrorResponse { Error = "Plan no encontrado" });
            }

            return NoContent();
        }

        /// <summary>
        /// Obtiene la suscripción de un tenant específico
        /// </summary>
        [HttpGet("tenants/{id}/suscripcion")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuscripcionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTenantSuscripcion(int id)
        {
            var suscripcion = await _suscripcionService.GetSuscripcionByNegocioAsync(id);
            if (suscripcion == null)
            {
                return NotFound(new ErrorResponse { Error = "El tenant no tiene suscripción activa" });
            }

            return Ok(suscripcion);
        }

        /// <summary>
        /// Crea o actualiza la suscripción de un tenant (para Super Admin)
        /// </summary>
        [HttpPost("tenants/{id}/suscripcion")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuscripcionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(SuscripcionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrUpdateTenantSuscripcion(int id, [FromBody] SuscripcionRequest request)
        {
            try
            {
                // Verificar que el tenant existe
                var tenant = await _superAdminService.GetTenantByIdAsync(id);
                if (tenant == null)
                {
                    return NotFound(new ErrorResponse { Error = "Tenant no encontrado" });
                }

                // Verificar si ya tiene suscripción
                var suscripcionExistente = await _suscripcionService.GetSuscripcionByNegocioAsync(id);
                
                SuscripcionResponse suscripcion;
                if (suscripcionExistente != null)
                {
                    // Actualizar suscripción existente
                    suscripcion = await _suscripcionService.UpdateSuscripcionAsync(id, request);
                    return Ok(suscripcion);
                }
                else
                {
                    // Crear nueva suscripción
                    suscripcion = await _suscripcionService.CreateSuscripcionAsync(id, request);
                    return CreatedAtAction(nameof(GetTenantSuscripcion), new { id }, suscripcion);
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }

        /// <summary>
        /// Cancela la suscripción de un tenant (para Super Admin)
        /// </summary>
        [HttpDelete("tenants/{id}/suscripcion")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuscripcionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelTenantSuscripcion(int id, [FromBody] CancelarSuscripcionRequest request)
        {
            try
            {
                var suscripcion = await _suscripcionService.CancelarSuscripcionAsync(id, request);
                return Ok(suscripcion);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }
    }
}
