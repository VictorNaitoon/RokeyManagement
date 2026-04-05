using API.Data;
using API.DTO.Request.Clientes;
using API.DTO.Response.Clientes;
using API.Models;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Clientes
{
    /// <summary>
    /// Implementación del servicio de gestión de clientes
    /// </summary>
    public class ClienteService : IClienteService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public ClienteService(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        /// <inheritdoc/>
        public async Task<List<AccountResponse>> GetAllAsync(int idNegocio, bool incluirConsumidorFinal = false, CancellationToken ct = default)
        {
            var query = _context.Clientes
                .Where(c => c.Id_negocio == idNegocio && c.Activo);

            // Filtrar "Consumidor Final" por defecto
            if (!incluirConsumidorFinal)
            {
                query = query.Where(c => c.Nombre != "Consumidor Final");
            }

            var clientes = await query
                .OrderBy(c => c.Nombre)
                .ToListAsync(ct);

            return clientes.Select(c => new AccountResponse(
                c.Id,
                c.Nombre,
                c.Apellido,
                c.Email ?? string.Empty,
                c.Documento,
                c.CondicionIVA,
                c.Telefono,
                c.Direccion,
                c.PermiteFiado,
                c.LimiteCredito,
                c.Activo
            )).ToList();
        }

        /// <inheritdoc/>
        public async Task<AccountResponse?> GetByIdAsync(int clienteId, int idNegocio, CancellationToken ct = default)
        {
            var cliente = await _context.Clientes
                .Where(c => c.Id == clienteId && c.Id_negocio == idNegocio && c.Activo)
                .FirstOrDefaultAsync(ct);

            if (cliente == null)
            {
                return null;
            }

            return new AccountResponse(
                cliente.Id,
                cliente.Nombre,
                cliente.Apellido,
                cliente.Email ?? string.Empty,
                cliente.Documento,
                cliente.CondicionIVA,
                cliente.Telefono,
                cliente.Direccion,
                cliente.PermiteFiado,
                cliente.LimiteCredito,
                cliente.Activo
            );
        }

        /// <inheritdoc/>
        public async Task<AccountResponse> CreateAsync(CrearClienteRequest request, int idNegocio, CancellationToken ct = default)
        {
            var cliente = new Cliente
            {
                Id_negocio = idNegocio,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Documento = request.Documento,
                CondicionIVA = request.CondicionIVA,
                Telefono = request.Telefono,
                Email = request.Email,
                Direccion = request.Direccion,
                FechaAlta = DateTime.UtcNow,
                PermiteFiado = request.PermiteFiado,
                LimiteCredito = request.LimiteCredito,
                Activo = true
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync(ct);

            return new AccountResponse(
                cliente.Id,
                cliente.Nombre,
                cliente.Apellido,
                cliente.Email ?? string.Empty,
                cliente.Documento,
                cliente.CondicionIVA,
                cliente.Telefono,
                cliente.Direccion,
                cliente.PermiteFiado,
                cliente.LimiteCredito,
                cliente.Activo
            );
        }

        /// <inheritdoc/>
        public async Task<AccountResponse?> UpdateAsync(int clienteId, ActualizarClienteRequest request, int idNegocio, CancellationToken ct = default)
        {
            var cliente = await _context.Clientes
                .Where(c => c.Id == clienteId && c.Id_negocio == idNegocio)
                .FirstOrDefaultAsync(ct);

            if (cliente == null)
            {
                return null;
            }

            if (request.Nombre != null) cliente.Nombre = request.Nombre;
            if (request.Apellido != null) cliente.Apellido = request.Apellido;
            if (request.Documento != null) cliente.Documento = request.Documento;
            if (request.CondicionIVA != null) cliente.CondicionIVA = request.CondicionIVA;
            if (request.Telefono != null) cliente.Telefono = request.Telefono;
            if (request.Email != null) cliente.Email = request.Email;
            if (request.Direccion != null) cliente.Direccion = request.Direccion;
            if (request.PermiteFiado.HasValue) cliente.PermiteFiado = request.PermiteFiado.Value;
            if (request.LimiteCredito.HasValue) cliente.LimiteCredito = request.LimiteCredito;

            await _context.SaveChangesAsync(ct);

            return new AccountResponse(
                cliente.Id,
                cliente.Nombre,
                cliente.Apellido,
                cliente.Email ?? string.Empty,
                cliente.Documento,
                cliente.CondicionIVA,
                cliente.Telefono,
                cliente.Direccion,
                cliente.PermiteFiado,
                cliente.LimiteCredito,
                cliente.Activo
            );
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int clienteId, int idNegocio, CancellationToken ct = default)
        {
            var cliente = await _context.Clientes
                .Where(c => c.Id == clienteId && c.Id_negocio == idNegocio)
                .FirstOrDefaultAsync(ct);

            if (cliente == null)
            {
                return false;
            }

            // Soft delete - desactivar en lugar de eliminar
            cliente.Activo = false;
            cliente.FechaBaja = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        /// <inheritdoc/>
        public async Task<VentaClienteListResponse> GetVentasAsync(int clienteId, int idNegocio, int page, int pageSize, CancellationToken ct = default)
        {
            // Validar que el cliente existe y pertenece al negocio
            var cliente = await _context.Clientes
                .Where(c => c.Id == clienteId && c.Id_negocio == idNegocio && c.Activo)
                .FirstOrDefaultAsync(ct);

            if (cliente == null)
            {
                return new VentaClienteListResponse();
            }

            // Query de ventas del cliente
            var ventasQuery = _context.Ventas
                .Where(v => v.IdCliente == clienteId && v.Id_negocio == idNegocio)
                .Include(v => v.DetallesVenta)
                .Include(v => v.Pagos)
                .OrderByDescending(v => v.FechaVenta);

            // Obtener total count
            var totalCount = await ventasQuery.CountAsync(ct);

            // Aplicar paginación
            var skip = (page - 1) * pageSize;
            var ventas = await ventasQuery
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(ct);

            // Mapear a VentaResumenResponse
            var items = ventas.Select(v => new VentaResumenResponse(
                v.Id,
                v.FechaVenta,
                v.TotalVenta,
                v.Anulada ? "Anulada" : "Activa",
                v.DetallesVenta.Sum(d => d.Cantidad),
                v.Pagos.Select(p => p.MetodoPago.ToString()).ToList()
            )).ToList();

            return new VentaClienteListResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <inheritdoc/>
        public async Task<SaldoClienteResponse> GetSaldoAsync(int clienteId, int idNegocio, CancellationToken ct = default)
        {
            // Obtener cliente con sus ventas y pagos
            var cliente = await _context.Clientes
                .Where(c => c.Id == clienteId && c.Id_negocio == idNegocio && c.Activo)
                .FirstOrDefaultAsync(ct);

            if (cliente == null)
            {
                return new SaldoClienteResponse(
                    0, "", 0, 0, 0, 0, 0, false, 0, new List<VentaResumenResponse>()
                );
            }

            // Obtener todas las ventas del cliente con sus pagos
            var ventas = await _context.Ventas
                .Where(v => v.IdCliente == clienteId && v.Id_negocio == idNegocio && !v.Anulada)
                .Include(v => v.Pagos)
                .Include(v => v.DetallesVenta)
                .ToListAsync(ct);

            // Calcular ventas pendientes (no pagadas completamente)
            var ventasPendientes = new List<VentaResumenResponse>();
            decimal totalVentasFiado = 0;
            decimal totalPagado = 0;

            foreach (var venta in ventas)
            {
                var montoPagado = venta.Pagos.Sum(p => p.Monto);
                
                // Una venta está pendiente si el monto pagado es menor al total de la venta
                if (montoPagado < venta.TotalVenta)
                {
                    totalVentasFiado += venta.TotalVenta;
                    totalPagado += montoPagado;

                    ventasPendientes.Add(new VentaResumenResponse(
                        venta.Id,
                        venta.FechaVenta,
                        venta.TotalVenta,
                        "Pendiente",
                        venta.DetallesVenta.Sum(d => d.Cantidad),
                        venta.Pagos.Select(p => p.MetodoPago.ToString()).ToList()
                    ));
                }
            }

            var saldoPendiente = totalVentasFiado - totalPagado;
            var limiteCredito = cliente.LimiteCredito ?? 0;
            var creditoDisponible = limiteCredito - saldoPendiente;
            if (creditoDisponible < 0) creditoDisponible = 0;

            return new SaldoClienteResponse(
                cliente.Id,
                cliente.Nombre,
                totalVentasFiado,
                totalPagado,
                saldoPendiente,
                limiteCredito,
                creditoDisponible,
                cliente.PermiteFiado,
                ventasPendientes.Count,
                ventasPendientes
            );
        }

        /// <inheritdoc/>
        public async Task<List<PagoClienteResponse>> GetPagosAsync(int clienteId, int idNegocio, CancellationToken ct = default)
        {
            // Validar que el cliente existe y pertenece al negocio
            var cliente = await _context.Clientes
                .Where(c => c.Id == clienteId && c.Id_negocio == idNegocio && c.Activo)
                .FirstOrDefaultAsync(ct);

            if (cliente == null)
            {
                return new List<PagoClienteResponse>();
            }

            // Obtener todos los pagos de todas las ventas del cliente
            var pagos = await _context.Pagos
                .Where(p => p.Venta.IdCliente == clienteId && p.Venta.Id_negocio == idNegocio)
                .Include(p => p.Venta)
                .OrderByDescending(p => p.Venta.FechaVenta)
                .ToListAsync(ct);

            // Mapear a PagoClienteResponse
            return pagos.Select(p => new PagoClienteResponse(
                p.Id,
                p.IdVenta,
                p.Venta.FechaVenta,
                p.MetodoPago.ToString(),
                p.Monto,
                null // NumeroComprobante no existe en el modelo actual
            )).ToList();
        }
    }
}
