using API.Data;
using API.DTO.Response.Suscripcion;
using API.Models;
using static API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using SuscripcionModel = API.Models.Suscripcion;

namespace API.Services.Suscripcion
{
    /// <summary>
    /// Implementación del servicio de gestión de pagos de suscripciones
    /// </summary>
    public class PagoSuscripcionService : IPagoSuscripcionService
    {
        private readonly AppDbContext _context;

        public PagoSuscripcionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PagoSuscripcionResponse>> GetPagosByNegocioAsync(int idNegocio, CancellationToken ct = default)
        {
            var pagos = await _context.PagosSuscripcion
                .Where(p => p.Suscripcion.Id_negocio == idNegocio)
                .OrderByDescending(p => p.FechaPago)
                .Select(p => new PagoSuscripcionResponse
                {
                    Id = p.Id,
                    IdSuscripcion = p.IdSuscripcion,
                    Monto = p.Monto,
                    FechaPago = p.FechaPago,
                    Metodo = p.Metodo,
                    Estado = p.Estado,
                    TransactionId = p.TransactionId,
                    Detalles = p.Detalles
                })
                .ToListAsync(ct);

            return pagos;
        }

        public async Task<PagoSuscripcionResponse?> GetPagoByIdAsync(int id, int idNegocio, CancellationToken ct = default)
        {
            var pago = await _context.PagosSuscripcion
                .Where(p => p.Id == id && p.Suscripcion.Id_negocio == idNegocio)
                .Select(p => new PagoSuscripcionResponse
                {
                    Id = p.Id,
                    IdSuscripcion = p.IdSuscripcion,
                    Monto = p.Monto,
                    FechaPago = p.FechaPago,
                    Metodo = p.Metodo,
                    Estado = p.Estado,
                    TransactionId = p.TransactionId,
                    Detalles = p.Detalles
                })
                .FirstOrDefaultAsync(ct);

            return pago;
        }

        public async Task<PagoSuscripcionResponse> CreatePagoAsync(int idSuscripcion, decimal monto, MetodoPagoSuscripcion metodo, string? referencia, CancellationToken ct = default)
        {
            // Validar que la suscripción existe
            var suscripcion = await _context.Suscripciones
                .FirstOrDefaultAsync(s => s.Id == idSuscripcion, ct);

            if (suscripcion == null)
            {
                throw new InvalidOperationException($"La suscripción con ID {idSuscripcion} no existe");
            }

            var pago = new PagoSuscripcion
            {
                IdSuscripcion = idSuscripcion,
                Monto = monto,
                FechaPago = DateTime.UtcNow,
                Metodo = metodo,
                TransactionId = referencia,
                Estado = EstadoPago.Exitoso
            };

            _context.PagosSuscripcion.Add(pago);
            await _context.SaveChangesAsync(ct);

            return new PagoSuscripcionResponse
            {
                Id = pago.Id,
                IdSuscripcion = pago.IdSuscripcion,
                Monto = pago.Monto,
                FechaPago = pago.FechaPago,
                Metodo = pago.Metodo,
                Estado = pago.Estado,
                TransactionId = pago.TransactionId,
                Detalles = pago.Detalles
            };
        }
    }
}
