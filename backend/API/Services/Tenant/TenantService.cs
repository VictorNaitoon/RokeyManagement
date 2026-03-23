using API.Data;
using API.Models;
using API.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Tenant
{
    public interface ITenantService
    {
        Task<(Negocio? negocio, Usuario? usuario, Suscripcion? suscripcion, string? error)> CreateTenantAsync(
            string email, 
            string password, 
            string nombre, 
            string apellido,
            string nombreNegocio, 
            string cuit,
            string? direccion,
            string? telefono,
            string tipoNegocio,
            int idPlan,
            string tipoFacturacion);

        Task<List<Plan>> GetPlanesActivosAsync();

        Task<(Suscripcion? suscripcion, string? error)> CreateSuscripcionAsync(
            int idNegocio,
            int idPlan,
            string tipoFacturacion,
            bool esTrial = false);

        Task<(PagoSuscripcion? pago, string? error)> ProcessPaymentAsync(
            int idSuscripcion,
            string metodoPago,
            string? paymentToken);
    }

    public class TenantService : ITenantService
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public TenantService(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<(Negocio? negocio, Usuario? usuario, Suscripcion? suscripcion, string? error)> CreateTenantAsync(
            string email,
            string password,
            string nombre,
            string apellido,
            string nombreNegocio,
            string cuit,
            string? direccion,
            string? telefono,
            string tipoNegocio,
            int idPlan,
            string tipoFacturacion)
        {
            // Verificar que el CUIT no exista en otro negocio
            var existingNegocio = await _context.Negocios.FirstOrDefaultAsync(n => n.CUIT == cuit);
            if (existingNegocio != null)
            {
                return (null, null, null, "Ya existe un negocio registrado con este CUIT");
            }

            // Verificar que el email no exista en la plataforma
            var existingUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                return (null, null, null, "Ya existe un usuario con este email");
            }

            // Verificar que el plan exista
            var plan = await _context.Planes.FindAsync(idPlan);
            if (plan == null)
            {
                return (null, null, null, "Plan no encontrado");
            }

            // Convertir tipo de negocio
            if (!Enum.TryParse<Enums.TipoNegocio>(tipoNegocio, true, out var tipoEnum))
            {
                tipoEnum = Enums.TipoNegocio.Ferreteria;
            }

            // Crear el negocio
            var negocio = new Negocio
            {
                Nombre = nombreNegocio,
                CUIT = cuit,
                Direccion = direccion ?? string.Empty,
                Telefono = telefono,
                Tipo = tipoEnum,
                Estado = Enums.EstadoNegocio.Activo,
                FechaInicioActividades = DateTime.UtcNow
            };

            _context.Negocios.Add(negocio);
            await _context.SaveChangesAsync();

            // Convertir tipo de facturación
            var tipoFacturacionEnum = tipoFacturacion == "Anual" 
                ? Enums.TipoFacturacion.Anual 
                : Enums.TipoFacturacion.Mensual;

            // Crear suscripción
            var fechaInicio = DateTime.UtcNow;
            var fechaProximoPago = tipoFacturacionEnum == Enums.TipoFacturacion.Anual
                ? fechaInicio.AddYears(1)
                : fechaInicio.AddMonths(1);

            var suscripcion = new Suscripcion
            {
                Id_negocio = negocio.Id,
                IdPlan = idPlan,
                FechaInicio = fechaInicio,
                FechaProximoPago = fechaProximoPago,
                Estado = Enums.EstadoSuscripcion.PendientePago,
                TipoFacturacion = tipoFacturacionEnum
            };

            _context.Suscripciones.Add(suscripcion);
            await _context.SaveChangesAsync();

            // Crear usuario admin del negocio
            var (usuario, userError) = await _authService.RegisterAsync(
                email,
                password,
                nombre,
                apellido,
                negocio.Id,
                Enums.RolUsuario.Dueño);

            if (userError != null)
            {
                return (negocio, null, suscripcion, userError);
            }

            return (negocio, usuario, suscripcion, null);
        }

        public async Task<List<Plan>> GetPlanesActivosAsync()
        {
            return await _context.Planes
                .Where(p => p.Activo)
                .OrderBy(p => p.Orden)
                .ToListAsync();
        }

        public async Task<(Suscripcion? suscripcion, string? error)> CreateSuscripcionAsync(
            int idNegocio,
            int idPlan,
            string tipoFacturacion,
            bool esTrial = false)
        {
            var plan = await _context.Planes.FindAsync(idPlan);
            if (plan == null)
            {
                return (null, "Plan no encontrado");
            }

            var tipoFacturacionEnum = tipoFacturacion == "Anual"
                ? Enums.TipoFacturacion.Anual
                : Enums.TipoFacturacion.Mensual;

            var fechaInicio = DateTime.UtcNow;
            var fechaProximoPago = tipoFacturacionEnum == Enums.TipoFacturacion.Anual
                ? fechaInicio.AddYears(1)
                : fechaInicio.AddMonths(1);

            var suscripcion = new Suscripcion
            {
                Id_negocio = idNegocio,
                IdPlan = idPlan,
                FechaInicio = fechaInicio,
                FechaProximoPago = fechaProximoPago,
                Estado = esTrial ? Enums.EstadoSuscripcion.Activa : Enums.EstadoSuscripcion.PendientePago,
                TipoFacturacion = tipoFacturacionEnum
            };

            _context.Suscripciones.Add(suscripcion);
            await _context.SaveChangesAsync();

            return (suscripcion, null);
        }

        public async Task<(PagoSuscripcion? pago, string? error)> ProcessPaymentAsync(
            int idSuscripcion,
            string metodoPago,
            string? paymentToken)
        {
            var suscripcion = await _context.Suscripciones
                .Include(s => s.Negocio)
                .FirstOrDefaultAsync(s => s.Id == idSuscripcion);

            if (suscripcion == null)
            {
                return (null, "Suscripción no encontrada");
            }

            var plan = await _context.Planes.FindAsync(suscripcion.IdPlan);
            if (plan == null)
            {
                return (null, "Plan no encontrado");
            }

            // Calcular monto
            var monto = suscripcion.TipoFacturacion == Enums.TipoFacturacion.Anual
                ? plan.PrecioAnual
                : plan.PrecioMensual;

            // Mock de procesamiento de pago (en producción, llamar a Mercado Pago API)
            var esExitoso = true;

            if (!Enum.TryParse<Enums.MetodoPagoSuscripcion>(metodoPago, true, out var metodoEnum))
            {
                metodoEnum = Enums.MetodoPagoSuscripcion.MercadoPago;
            }

            var pago = new PagoSuscripcion
            {
                IdSuscripcion = idSuscripcion,
                Monto = monto,
                FechaPago = DateTime.UtcNow,
                Metodo = metodoEnum,
                TransactionId = esExitoso ? Guid.NewGuid().ToString() : null,
                Estado = esExitoso ? Enums.EstadoPago.Exitoso : Enums.EstadoPago.Fallido,
                Detalles = esExitoso ? "Pago procesado correctamente" : "Error en el procesamiento del pago"
            };

            _context.PagosSuscripcion.Add(pago);

            // Actualizar estado de suscripción
            if (esExitoso)
            {
                suscripcion.Estado = Enums.EstadoSuscripcion.Activa;
            }
            else
            {
                suscripcion.Estado = Enums.EstadoSuscripcion.PendientePago;
            }

            await _context.SaveChangesAsync();

            if (!esExitoso)
            {
                return (pago, "Error al procesar el pago. Por favor, verifique los datos e intente nuevamente.");
            }

            return (pago, null);
        }
    }
}
