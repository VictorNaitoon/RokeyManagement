namespace API.DTO.Response.Suscripcion
{
    /// <summary>
    /// DTO para el historial de pagos de suscripción
    /// </summary>
    public class PagoSuscripcionResponse
    {
        public int Id { get; set; }
        public int IdSuscripcion { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public Models.Enums.MetodoPagoSuscripcion Metodo { get; set; }
        public Models.Enums.EstadoPago Estado { get; set; }
        public string? TransactionId { get; set; }
        public string? Detalles { get; set; }
    }
}
