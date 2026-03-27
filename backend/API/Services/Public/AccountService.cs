using API.Data;
using API.DTO.Request.Public;
using API.DTO.Response.Public;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Publicos
{
    /// <summary>
    /// Implementación del servicio de gestión de clientes (Admin)
    /// </summary>
    public class CuentaPublicaService : ICuentaPublicaService
    {
        private readonly AppDbContext _context;

        public CuentaPublicaService(AppDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<List<AccountResponse>> GetClientesAsync(int idNegocio)
        {
            var clientes = await _context.Clientes
                .Where(c => c.Id_negocio == idNegocio)
                .OrderBy(c => c.Nombre)
                .Select(c => new AccountResponse(
                    c.Id,
                    c.Nombre,
                    c.Apellido,
                    c.Email ?? string.Empty,
                    null,
                    null
                ))
                .ToListAsync();

            return clientes;
        }

        /// <inheritdoc/>
        public async Task<AccountResponse> CrearClienteAsync(CrearClienteRequest request, int idNegocio)
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
            await _context.SaveChangesAsync();

            return new AccountResponse(
                cliente.Id,
                cliente.Nombre,
                cliente.Apellido,
                cliente.Email ?? string.Empty,
                null,
                new ClienteInfoResponse(cliente.PermiteFiado, cliente.LimiteCredito)
            );
        }

        /// <inheritdoc/>
        public async Task<AccountResponse?> ActualizarClienteAsync(int clienteId, ActualizarClienteRequest request, int idNegocio)
        {
            var cliente = await _context.Clientes
                .Where(c => c.Id == clienteId && c.Id_negocio == idNegocio)
                .FirstOrDefaultAsync();

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

            await _context.SaveChangesAsync();

            return new AccountResponse(
                cliente.Id,
                cliente.Nombre,
                cliente.Apellido,
                cliente.Email ?? string.Empty,
                null,
                new ClienteInfoResponse(cliente.PermiteFiado, cliente.LimiteCredito)
            );
        }

        /// <inheritdoc/>
        public async Task<bool> EliminarClienteAsync(int clienteId, int idNegocio)
        {
            var cliente = await _context.Clientes
                .Where(c => c.Id == clienteId && c.Id_negocio == idNegocio)
                .FirstOrDefaultAsync();

            if (cliente == null)
            {
                return false;
            }

            // Hard delete - eliminar completamente de la base de datos
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}