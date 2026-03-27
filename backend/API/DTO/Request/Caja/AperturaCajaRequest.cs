namespace API.DTO.Request.Caja
{
    public class AperturaCajaRequest
    {
        /// <summary>
        /// Monto inicial con el que se abre la caja. Debe ser mayor o igual a cero.
        /// </summary>
        public decimal MontoInicial { get; set; }

        /// <summary>
        /// Observaciones opcionales al abrir la caja.
        /// </summary>
        public string? Observaciones { get; set; }
    }
}
