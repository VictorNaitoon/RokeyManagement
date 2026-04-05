using System.Text.Json;
using API.Data;
using API.DTO.Request.Auditoria;
using API.DTO.Response.Auditoria;
using API.Models;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Auditoria
{
    /// <summary>
    /// Servicio de auditoría que registra cambios en las entidades del sistema
    /// </summary>
    public class AuditoriaService : IAuditoriaService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public AuditoriaService(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Registra una acción de auditoría con los datos antes y después del cambio
        /// </summary>
        public async Task RegistrarAsync(
            string entidad,
            int idRegistro,
            string accion,
            object? datosAnteriores = null,
            object? datosNuevos = null,
            CancellationToken ct = default)
        {
            var auditoria = new API.Models.Auditoria
            {
                Entidad = entidad,
                IdRegistro = idRegistro,
                Accion = accion.ToUpperInvariant(),
                IdUsuario = _currentUser.UserId,
                Fecha = DateTime.UtcNow,
                DatosAnteriores = datosAnteriores != null ? JsonSerializer.Serialize(datosAnteriores, JsonOptions) : null,
                DatosNuevos = datosNuevos != null ? JsonSerializer.Serialize(datosNuevos, JsonOptions) : null,
                Id_negocio = _currentUser.NegocioId
            };

            _context.Auditorias.Add(auditoria);
            await _context.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Lista registros de auditoría con filtros y paginación
        /// </summary>
        public async Task<(List<AuditoriaListResponse> Items, int Total)> ListarAsync(
            FiltroAuditoriaRequest filtro,
            CancellationToken ct = default)
        {
            var query = _context.Auditorias
                .Where(a => a.Id_negocio == _currentUser.NegocioId)
                .Include(a => a.Usuario)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(filtro.Entidad))
            {
                query = query.Where(a => a.Entidad == filtro.Entidad);
            }

            if (filtro.IdUsuario.HasValue)
            {
                query = query.Where(a => a.IdUsuario == filtro.IdUsuario.Value);
            }

            if (filtro.FechaDesde.HasValue)
            {
                query = query.Where(a => a.Fecha >= filtro.FechaDesde.Value);
            }

            if (filtro.FechaHasta.HasValue)
            {
                query = query.Where(a => a.Fecha <= filtro.FechaHasta.Value.Date.AddDays(1).AddTicks(-1));
            }

            if (filtro.IdRegistro.HasValue)
            {
                query = query.Where(a => a.IdRegistro == filtro.IdRegistro.Value);
            }

            // Obtener total antes de paginar
            var total = await query.CountAsync(ct);

            // Ordenar por fecha descendente y paginar
            var items = await query
                .OrderByDescending(a => a.Fecha)
                .Skip((filtro.Page - 1) * filtro.PageSize)
                .Take(Math.Min(filtro.PageSize, 100))
                .Select(a => new AuditoriaListResponse
                {
                    Id = a.Id,
                    Entidad = a.Entidad,
                    IdRegistro = a.IdRegistro,
                    Accion = a.Accion,
                    IdUsuario = a.IdUsuario,
                    NombreUsuario = a.Usuario != null ? a.Usuario.Nombre + " " + a.Usuario.Apellido : null,
                    Fecha = a.Fecha
                })
                .ToListAsync(ct);

            return (items, total);
        }

        /// <summary>
        /// Obtiene un registro de auditoría por ID
        /// </summary>
        public async Task<AuditoriaResponse?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            var auditoria = await _context.Auditorias
                .Where(a => a.Id == id && a.Id_negocio == _currentUser.NegocioId)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(ct);

            if (auditoria == null) return null;

            return new AuditoriaResponse
            {
                Id = auditoria.Id,
                Entidad = auditoria.Entidad,
                IdRegistro = auditoria.IdRegistro,
                Accion = auditoria.Accion,
                IdUsuario = auditoria.IdUsuario,
                NombreUsuario = auditoria.Usuario != null ? auditoria.Usuario.Nombre + " " + auditoria.Usuario.Apellido : null,
                Fecha = auditoria.Fecha,
                DatosAnteriores = auditoria.DatosAnteriores,
                DatosNuevos = auditoria.DatosNuevos
            };
        }
    }
}