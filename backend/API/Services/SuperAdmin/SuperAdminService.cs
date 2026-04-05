using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using SuscripcionModel = API.Models.Suscripcion;

namespace API.Services.SuperAdmin
{
    public interface ISuperAdminService
    {
        // Gestión de Tenants
        Task<List<Negocio>> GetAllTenantsAsync();
        Task<Negocio?> GetTenantByIdAsync(int id);
        Task<Negocio?> CreateTenantAsync(
            string emailAdmin,
            string passwordAdmin,
            string nombreAdmin,
            string apellidoAdmin,
            string nombre,
            string cuit,
            string? direccion,
            string? logoUrl,
            string? telefono,
            string? email,
            string? puntoVenta,
            string? condicionVentas,
            Enums.TipoNegocio tipo,
            bool activo,
            int idPlan,
            string tipoFacturacion,
            bool activarSuscripcion);
        Task<Negocio?> UpdateTenantEstadoAsync(int id, Enums.EstadoNegocio estado);
        Task<DashboardMetrics> GetDashboardMetricsAsync();

        // Gestión de Planes
        Task<List<Plan>> GetAllPlanesAsync();
        Task<Plan?> CreatePlanAsync(Plan plan);
        Task<Plan?> UpdatePlanAsync(int id, Plan plan);
        Task<bool> DeletePlanAsync(int id);

        // Métricas de la plataforma
        Task<PlatformMetrics> GetPlatformMetricsAsync();
    }

    public class DashboardMetrics
    {
        public int TotalTenants { get; set; }
        public int TenantsActivos { get; set; }
        public int TenantsInactivos { get; set; }
        public int TenantsTrial { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalProductos { get; set; }
    }

    public class PlatformMetrics
    {
        public int TotalTenants { get; set; }
        public int TenantsActivos { get; set; }
        public decimal IngresosMensuales { get; set; }
        public decimal IngresosAnuales { get; set; }
        public int TotalTransacciones { get; set; }
        public List<MonthlyRevenue> IngresosPorMes { get; set; } = new();
    }

    public class MonthlyRevenue
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public decimal Ingresos { get; set; }
    }

    public class SuperAdminService : ISuperAdminService
    {
        private readonly AppDbContext _context;

        public SuperAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Negocio>> GetAllTenantsAsync()
        {
            return await _context.Negocios
                .Include(n => n.Usuarios)
                .Include(n => n.Productos)
                .OrderByDescending(n => n.FechaInicioActividades)
                .ToListAsync();
        }

        public async Task<Negocio?> GetTenantByIdAsync(int id)
        {
            return await _context.Negocios
                .Include(n => n.Usuarios)
                .Include(n => n.Productos)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        /// <summary>
        /// Crea un nuevo negocio con su usuario admin y suscripción
        /// (Usado por el Super Admin para registrar negocios manualmente)
        /// </summary>
        public async Task<Negocio?> CreateTenantAsync(
            string emailAdmin,
            string passwordAdmin,
            string nombreAdmin,
            string apellidoAdmin,
            string nombre,
            string cuit,
            string? direccion,
            string? logoUrl,
            string? telefono,
            string? email,
            string? puntoVenta,
            string? condicionVentas,
            Enums.TipoNegocio tipo,
            bool activo,
            int idPlan,
            string tipoFacturacion,
            bool activarSuscripcion)
        {
            // Verificar que el CUIT no exista
            var existingNegocio = await _context.Negocios.FirstOrDefaultAsync(n => n.CUIT == cuit);
            if (existingNegocio != null)
            {
                throw new InvalidOperationException("Ya existe un negocio registrado con este CUIT");
            }

            // Verificar que el email no exista
            var existingUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailAdmin);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Ya existe un usuario con este email");
            }

            // Verificar que el plan exista
            var plan = await _context.Planes.FindAsync(idPlan);
            if (plan == null)
            {
                throw new InvalidOperationException("Plan no encontrado");
            }

            // Crear el negocio
            var negocio = new Negocio
            {
                Nombre = nombre,
                CUIT = cuit,
                Direccion = direccion ?? string.Empty,
                LogoURL = logoUrl,
                Telefono = telefono,
                Email = email,
                PuntoDeVenta = puntoVenta,
                CondicionVentas = condicionVentas,
                Tipo = tipo,
                Estado = activo ? Enums.EstadoNegocio.Activo : Enums.EstadoNegocio.Inactivo,
                FechaInicioActividades = DateTime.UtcNow,
                IngresosBrutos = 0
            };

            _context.Negocios.Add(negocio);
            await _context.SaveChangesAsync();

                // Crear usuario admin del negocio
                var passwordHash = Services.Auth.AuthService.HashPassword(passwordAdmin);
            var usuarioAdmin = new Usuario
            {
                Id_negocio = negocio.Id,
                Email = emailAdmin,
                PasswordHash = passwordHash,
                Nombre = nombreAdmin,
                Apellido = apellidoAdmin,
                Rol = Enums.RolUsuario.Dueño,
                Activo = true
            };

            _context.Usuarios.Add(usuarioAdmin);
            await _context.SaveChangesAsync();

            // Crear suscripción
            var tipoFacturacionEnum = tipoFacturacion == "Anual"
                ? Enums.TipoFacturacion.Anual
                : Enums.TipoFacturacion.Mensual;

            var fechaInicio = DateTime.UtcNow;
            var fechaProximoPago = tipoFacturacionEnum == Enums.TipoFacturacion.Anual
                ? fechaInicio.AddYears(1)
                : fechaInicio.AddMonths(1);

            var suscripcion = new SuscripcionModel
            {
                Id_negocio = negocio.Id,
                IdPlan = idPlan,
                FechaInicio = fechaInicio,
                FechaProximoPago = fechaProximoPago,
                Estado = activarSuscripcion
                    ? Enums.EstadoSuscripcion.Activa
                    : Enums.EstadoSuscripcion.PendientePago,
                TipoFacturacion = tipoFacturacionEnum
            };

            _context.Suscripciones.Add(suscripcion);
            await _context.SaveChangesAsync();

            return negocio;
        }

        public async Task<Negocio?> UpdateTenantEstadoAsync(int id, Enums.EstadoNegocio estado)
        {
            var negocio = await _context.Negocios.FindAsync(id);
            if (negocio == null) return null;

            negocio.Estado = estado;
            await _context.SaveChangesAsync();
            return negocio;
        }

        public async Task<DashboardMetrics> GetDashboardMetricsAsync()
        {
            var negocios = await _context.Negocios.ToListAsync();
            var usuarios = await _context.Usuarios.ToListAsync();
            var productos = await _context.Productos.ToListAsync();

            return new DashboardMetrics
            {
                TotalTenants = negocios.Count,
                TenantsActivos = negocios.Count(n => n.Estado == Enums.EstadoNegocio.Activo),
                TenantsInactivos = negocios.Count(n => n.Estado == Enums.EstadoNegocio.Inactivo),
                TenantsTrial = 0, // Temporalmente deshabilitado
                TotalUsuarios = usuarios.Count,
                TotalProductos = productos.Count
            };
        }

        public async Task<List<Plan>> GetAllPlanesAsync()
        {
            return await _context.Planes
                .OrderBy(p => p.Orden)
                .ToListAsync();
        }

        public async Task<Plan?> CreatePlanAsync(Plan plan)
        {
            _context.Planes.Add(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<Plan?> UpdatePlanAsync(int id, Plan planActualizado)
        {
            var plan = await _context.Planes.FindAsync(id);
            if (plan == null) return null;

            plan.Nombre = planActualizado.Nombre;
            plan.Descripcion = planActualizado.Descripcion;
            plan.PrecioMensual = planActualizado.PrecioMensual;
            plan.PrecioAnual = planActualizado.PrecioAnual;
            plan.MaxUsuarios = planActualizado.MaxUsuarios;
            plan.MaxProductos = planActualizado.MaxProductos;
            plan.MaxTransaccionesMes = planActualizado.MaxTransaccionesMes;
            plan.SoportePrioritario = planActualizado.SoportePrioritario;
            plan.MultiSucursal = planActualizado.MultiSucursal;
            plan.APIAccess = planActualizado.APIAccess;
            plan.Activo = planActualizado.Activo;
            plan.Orden = planActualizado.Orden;

            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<bool> DeletePlanAsync(int id)
        {
            var plan = await _context.Planes.FindAsync(id);
            if (plan == null) return false;

            // Verificar que no haya suscripciones con este plan
            var tieneSuscripciones = await _context.Suscripciones.AnyAsync(s => s.IdPlan == id);
            if (tieneSuscripciones)
            {
                // En lugar de eliminar, desactivarlo
                plan.Activo = false;
                await _context.SaveChangesAsync();
                return true;
            }

            _context.Planes.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PlatformMetrics> GetPlatformMetricsAsync()
        {
            var negocios = await _context.Negocios.ToListAsync();
            var pagos = await _context.PagosSuscripcion
                .Where(p => p.Estado == Enums.EstadoPago.Exitoso)
                .ToListAsync();

            var planes = await _context.Planes.ToListAsync();

            // Calcular ingresos mensuales (último mes)
            var mesActual = DateTime.UtcNow.Month;
            var añoActual = DateTime.UtcNow.Year;
            var pagosMesActual = pagos
                .Where(p => p.FechaPago.Month == mesActual && p.FechaPago.Year == añoActual)
                .Sum(p => p.Monto);

            // Calcular ingresos anuales
            var pagosAñoActual = pagos
                .Where(p => p.FechaPago.Year == añoActual)
                .Sum(p => p.Monto);

            return new PlatformMetrics
            {
                TotalTenants = negocios.Count,
                TenantsActivos = negocios.Count(n => n.Estado == Enums.EstadoNegocio.Activo),
                IngresosMensuales = pagosMesActual,
                IngresosAnuales = pagosAñoActual,
                TotalTransacciones = pagos.Count,
                IngresosPorMes = new List<MonthlyRevenue>()
            };
        }
    }
}
