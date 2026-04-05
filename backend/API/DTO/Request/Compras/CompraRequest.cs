namespace API.DTO.Request.Compras
{
    public class CrearCompraRequest
    {
        /// <summary>
        /// Número de comprobante o factura del proveedor (obligatorio)
        /// </summary>
        public string NumeroComprobante { get; set; } = string.Empty;

        /// <summary>
        /// ID del proveedor al que se le realiza la compra (obligatorio)
        /// </summary>
        public int IdProveedor { get; set; }

        /// <summary>
        /// Lista de productos comprados (al menos uno requerido)
        /// </summary>
        public List<DetalleCompraRequest> Detalles { get; set; } = new List<DetalleCompraRequest>();
    }

    public class DetalleCompraRequest
    {
        /// <summary>
        /// ID del producto que se compra (obligatorio)
        /// </summary>
        public int IdProducto { get; set; }

        /// <summary>
        /// Cantidad comprada del producto (mayor a cero)
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Precio unitario del producto en esta compra (mayor a cero)
        /// </summary>
        public decimal PrecioUnitario { get; set; }
    }

    public class AnularCompraRequest
    {
        /// <summary>
        /// Motivo opcional de la anulación
        /// </summary>
        public string? Motivo { get; set; }
    }
}