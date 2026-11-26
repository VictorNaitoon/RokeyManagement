namespace API.Services.Ventas
{
    /// <summary>
    /// Archivo generado para compartir o descargar el comprobante de una venta.
    /// </summary>
    public record ComprobanteVentaFile(byte[] Content, string ContentType, string FileName);

    public interface IComprobanteVentaService
    {
        /// <summary>
        /// Genera el comprobante de la venta en formato "pdf" o "png".
        /// Devuelve null si la venta no existe o pertenece a otro negocio.
        /// </summary>
        Task<ComprobanteVentaFile?> GenerarAsync(int ventaId, string formato, CancellationToken ct);
    }
}
