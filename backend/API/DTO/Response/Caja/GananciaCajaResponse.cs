namespace API.DTO.Response.Caja
{
    /// <summary>
    /// Ganancia real de una caja: margen por producto vendido.
    /// Agregado seguro para todos los roles (no expone PrecioCompra por producto).
    /// </summary>
    public class GananciaCajaResponse
    {
        /// <summary>
        /// Suma de (PrecioUnitario - PrecioCompra) * Cantidad de las ventas activas de la caja.
        /// </summary>
        public decimal Ganancia { get; set; }

        /// <summary>
        /// Cantidad de ventas activas consideradas en el cálculo.
        /// </summary>
        public int VentasCantidad { get; set; }
    }
}
