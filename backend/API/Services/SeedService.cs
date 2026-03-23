using API.Data;
using API.Models;
using API.Services.Auth;

namespace API.Services
{
    public class SeedService
    {
        private readonly AppDbContext _context;

        public SeedService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedPlanesAsync()
        {
            if (_context.Planes.Any())
            {
                return; // Ya hay planes insertados
            }

            var planes = new List<Plan>
            {
                new Plan
                {
                    Nombre = "Básico",
                    Descripcion = "Ideal para emprendimientos individuales o pequeños negocios. Incluye las funcionalidades esenciales para gestionar tu ferretería o cerrajería.",
                    PrecioMensual = 9.99m,
                    PrecioAnual = 99.90m,
                    MaxUsuarios = 1,
                    MaxProductos = 500,
                    MaxTransaccionesMes = 100,
                    SoportePrioritario = false,
                    MultiSucursal = false,
                    APIAccess = false,
                    Activo = true,
                    Orden = 1
                },
                new Plan
                {
                    Nombre = "Profesional",
                    Descripcion = "Perfecto para negocios en crecimiento. Soporta hasta 5 usuarios, productos ilimitados y todas las funcionalidades avanzadas incluyendo catálogo público y facturación.",
                    PrecioMensual = 24.99m,
                    PrecioAnual = 249.90m,
                    MaxUsuarios = 5,
                    MaxProductos = 999999,
                    MaxTransaccionesMes = 1000,
                    SoportePrioritario = true,
                    MultiSucursal = false,
                    APIAccess = false,
                    Activo = true,
                    Orden = 2
                },
                new Plan
                {
                    Nombre = "Enterprise",
                    Descripcion = "Solución completa para grandes operaciones. Usuarios ilimitados, multi-sucursal, acceso a API y soporte prioritario 24/7.",
                    PrecioMensual = 49.99m,
                    PrecioAnual = 499.90m,
                    MaxUsuarios = 999999,
                    MaxProductos = 999999,
                    MaxTransaccionesMes = 999999,
                    SoportePrioritario = true,
                    MultiSucursal = true,
                    APIAccess = true,
                    Activo = true,
                    Orden = 3
                }
            };

            _context.Planes.AddRange(planes);
            await _context.SaveChangesAsync();
        }

        public async Task SeedSuperAdminAsync()
        {
            if (_context.SuperAdmins.Any())
            {
                return; // Ya hay Super Admin
            }

            // Credenciales por defecto: admin@rokeystore.com / RoKey2026!
            var superAdmin = new Models.SuperAdmin
            {
                Email = "admin@rokeystore.com",
                PasswordHash = AuthService.HashPassword("RoKey2026!"),
                Rol = "SuperAdmin",
                Activo = true
            };

            _context.SuperAdmins.Add(superAdmin);
            await _context.SaveChangesAsync();
        }
    }
}
