namespace API.DTO.Request.Suscripcion
{
    /// <summary>
    /// DTO para cancelar una suscripción
    /// </summary>
    public class CancelarSuscripcionRequest
    {
        public string? MotivoCancelacion { get; set; }
    }
}
