namespace API.DTO.Request.Caja
{
    public class AgregarMovimientoCajaRequest
    {
        /// <summary>
        /// Tipo de movimiento: 'Ingreso' o 'Egreso'.
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Monto del movimiento. Debe ser mayor a cero.
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Descripción opcional del movimiento (máximo 500 caracteres).
        /// </summary>
        public string? Descripcion { get; set; }
    }
}
