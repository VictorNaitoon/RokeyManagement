using API.Data;
using API.DTO.Request.Negocios;
using API.DTO.Response.Negocios;
using API.Models;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Negocios
{
    public interface INegocioService
    {
        Task<NegocioResponse?> GetMiNegocioAsync();
        Task<NegocioResponse?> UpdateMiNegocioAsync(ActualizarNegocioRequest request);
    }

    public class NegocioService : INegocioService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public NegocioService(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene los datos del negocio al que pertenece el usuario actual
        /// </summary>
        public async Task<NegocioResponse?> GetMiNegocioAsync()
        {
            // El SuperAdmin no tiene un negocio propio
            if (_currentUser.NegocioId == 0 && _currentUser.Rol == "SuperAdmin")
            {
                return null;
            }

            var negocio = await _context.Negocios
                .Include(n => n.Usuarios)
                .Include(n => n.Productos)
                .FirstOrDefaultAsync(n => n.Id == _currentUser.NegocioId);

            if (negocio == null) return null;

            return MapToResponse(negocio);
        }

        /// <summary>
        /// Actualiza los datos del negocio al que pertenece el usuario actual
        /// Solo el Dueño puede actualizar
        /// </summary>
        public async Task<NegocioResponse?> UpdateMiNegocioAsync(ActualizarNegocioRequest request)
        {
            // Solo el Dueño puede modificar su negocio
            if (!_currentUser.IsAdmin)
            {
                throw new InvalidOperationException("Solo el administrador del negocio puede modificar los datos");
            }

            // El SuperAdmin no tiene un negocio propio
            if (_currentUser.NegocioId == 0 && _currentUser.Rol == "SuperAdmin")
            {
                throw new InvalidOperationException("El Super Admin no tiene un negocio asociado");
            }

            var negocio = await _context.Negocios
                .Include(n => n.Usuarios)
                .Include(n => n.Productos)
                .FirstOrDefaultAsync(n => n.Id == _currentUser.NegocioId);

            if (negocio == null)
            {
                throw new InvalidOperationException("Negocio no encontrado");
            }

            // Verificar que el CUIT no esté en uso por otro negocio
            var existingNegocio = await _context.Negocios
                .FirstOrDefaultAsync(n => n.CUIT == request.CUIT && n.Id != _currentUser.NegocioId);

            if (existingNegocio != null)
            {
                throw new InvalidOperationException("Ya existe otro negocio registrado con este CUIT");
            }

            // Actualizar campos
            negocio.Nombre = request.Nombre;
            negocio.CUIT = request.CUIT;
            negocio.Direccion = request.Direccion;
            negocio.LogoURL = request.LogoURL;
            negocio.Telefono = request.Telefono;
            negocio.Email = request.Email;
            negocio.PuntoDeVenta = request.PuntoDeVenta;
            negocio.CondicionVentas = request.CondicionVentas;
            negocio.Tipo = (Enums.TipoNegocio)request.Tipo;

            await _context.SaveChangesAsync();

            // Recargar para obtener conteos
            await _context.Entry(negocio).Collection(n => n.Usuarios).LoadAsync();
            await _context.Entry(negocio).Collection(n => n.Productos).LoadAsync();

            return MapToResponse(negocio);
        }

        private static NegocioResponse MapToResponse(Negocio negocio)
        {
            return new NegocioResponse
            {
                Id = negocio.Id,
                Nombre = negocio.Nombre,
                CUIT = negocio.CUIT,
                Direccion = negocio.Direccion,
                LogoURL = negocio.LogoURL,
                Estado = negocio.Estado.ToString(),
                Tipo = negocio.Tipo.ToString(),
                IngresosBrutos = negocio.IngresosBrutos,
                FechaInicioActividades = negocio.FechaInicioActividades,
                PuntoDeVenta = negocio.PuntoDeVenta,
                Telefono = negocio.Telefono,
                Email = negocio.Email,
                CondicionVentas = negocio.CondicionVentas,
                TotalUsuarios = negocio.Usuarios?.Count ?? 0,
                TotalProductos = negocio.Productos?.Count ?? 0
            };
        }
    }
}