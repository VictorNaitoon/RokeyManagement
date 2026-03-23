using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.DTO.Request;
using API.DTO.Response;
using API.DTO.Response.Tenant;
using API.Models;
using API.Services.Auth;
using API.Services.Tenant;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.Tenant
{
    [ApiController]
    [Route("api/v1/public")]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IJwtService _jwtService;
        private readonly AppDbContext _context;

        public TenantController(
            ITenantService tenantService, 
            IJwtService jwtService,
            AppDbContext context)
        {
            _tenantService = tenantService;
            _jwtService = jwtService;
            _context = context;
        }

        /// <summary>
        /// Obtener los planes disponibles para suscripción
        /// </summary>
        [HttpGet("planes")]
        [ProducesResponseType(typeof(List<TenantPlanResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlanes()
        {
            var planes = await _tenantService.GetPlanesActivosAsync();
            
            var response = planes.Select(p => new TenantPlanResponse
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PrecioMensual = p.PrecioMensual,
                PrecioAnual = p.PrecioAnual
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Registrar un nuevo tenant (negocio) con suscripción
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterTenantResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterTenant([FromBody] RegisterTenantRequest request)
        {
            var result = await _tenantService.CreateTenantAsync(
                request.Email,
                request.Password,
                request.Nombre,
                request.Apellido,
                request.NombreNegocio,
                request.CUIT,
                request.Direccion,
                request.Telefono,
                request.TipoNegocio,
                request.IdPlan,
                request.TipoFacturacion);

            if (result.error != null)
            {
                return BadRequest(new ErrorResponse { Error = result.error });
            }

            if (result.negocio == null || result.usuario == null || result.suscripcion == null)
            {
                return BadRequest(new ErrorResponse { Error = "Error al crear el negocio" });
            }

            // Obtener nombre del plan
            var plan = await _context.Planes.FindAsync(request.IdPlan);
            var planNombre = plan?.Nombre ?? "Desconocido";

            // Generar token
            var token = _jwtService.GenerateToken(result.usuario);

            return CreatedAtAction(nameof(RegisterTenant), new RegisterTenantResponse
            {
                IdNegocio = result.negocio.Id,
                NombreNegocio = result.negocio.Nombre,
                IdUsuario = result.usuario.Id,
                Email = result.usuario.Email,
                IdSuscripcion = result.suscripcion.Id,
                Plan = planNombre,
                Token = token,
                Message = "Negocio registrado exitosamente. Por favor, complete el pago para activar la suscripción."
            });
        }

        /// <summary>
        /// Iniciar período de trial (14 días gratis) - Plan Básico
        /// </summary>
        [HttpPost("trial")]
        [ProducesResponseType(typeof(RegisterTenantResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StartTrial([FromBody] StartTrialRequest request)
        {
            // Trial usa el plan Básico (id = 1)
            var result = await _tenantService.CreateTenantAsync(
                request.Email,
                request.Password,
                request.Nombre,
                request.Apellido,
                request.NombreNegocio,
                request.CUIT,
                request.Direccion,
                request.Telefono,
                request.TipoNegocio,
                idPlan: 1,
                tipoFacturacion: "Mensual");

            if (result.error != null)
            {
                return BadRequest(new ErrorResponse { Error = result.error });
            }

            if (result.negocio == null || result.usuario == null || result.suscripcion == null)
            {
                return BadRequest(new ErrorResponse { Error = "Error al crear el negocio" });
            }

            // Actualizar suscripción a activa (trial)
            result.suscripcion.Estado = Models.Enums.EstadoSuscripcion.Activa;
            await _context.SaveChangesAsync();

            // Generar token
            var token = _jwtService.GenerateToken(result.usuario);

            return CreatedAtAction(nameof(StartTrial), new RegisterTenantResponse
            {
                IdNegocio = result.negocio.Id,
                NombreNegocio = result.negocio.Nombre,
                IdUsuario = result.usuario.Id,
                Email = result.usuario.Email,
                IdSuscripcion = result.suscripcion.Id,
                Plan = "Básico",
                Token = token,
                Message = "Trial de 14 días iniciado. Explora todas las funcionalidades."
            });
        }

        /// <summary>
        /// Procesar pago de suscripción
        /// </summary>
        [HttpPost("payment")]
        [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            var (pago, error) = await _tenantService.ProcessPaymentAsync(
                request.IdSuscripcion,
                request.MetodoPago,
                request.PaymentToken);

            if (error != null)
            {
                return BadRequest(new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = error
                });
            }

            return Ok(new PaymentResponse
            {
                Success = true,
                TransactionId = pago?.TransactionId,
                Message = "Pago procesado correctamente"
            });
        }
    }
}
