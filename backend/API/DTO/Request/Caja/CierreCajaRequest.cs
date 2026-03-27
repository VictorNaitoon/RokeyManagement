namespace API.DTO.Request.Caja
{
    public class CierreCajaRequest
    {
        /// <summary>
        /// Monto final con el que se cierra la caja.
        /// </summary>
        public decimal MontoFinal { get; set; }

        /// <summary>
        /// Observaciones opcionales al cerrar la caja.
        /// </summary>
        public string? Observaciones { get; set; }
    }
}
