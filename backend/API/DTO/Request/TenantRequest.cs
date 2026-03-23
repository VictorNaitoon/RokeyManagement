namespace API.DTO.Request
{
    /// <summary>
    /// Request para el registro completo de un nuevo tenant (negocio)
    /// </summary>
    public class RegisterTenantRequest
    {
        // Datos del usuario admin
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;

        // Datos del negocio
        public string NombreNegocio { get; set; } = string.Empty;
        public string CUIT { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string TipoNegocio { get; set; } = "Ferreteria"; // Ferreteria, Cerrajeria, Ambos

        // Datos de la suscripción
        public int IdPlan { get; set; }
        public string TipoFacturacion { get; set; } = "Mensual"; // Mensual, Anual
    }

    /// <summary>
    /// Response del registro de tenant
    /// </summary>
    public class RegisterTenantResponse
    {
        public int IdNegocio { get; set; }
        public string NombreNegocio { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public string Email { get; set; } = string.Empty;
        public int IdSuscripcion { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para iniciar trial (sin pago)
    /// </summary>
    public class StartTrialRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NombreNegocio { get; set; } = string.Empty;
        public string CUIT { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string TipoNegocio { get; set; } = "Ferreteria";
    }

    /// <summary>
    /// Request para procesar pago de suscripción
    /// </summary>
    public class ProcessPaymentRequest
    {
        public int IdSuscripcion { get; set; }
        public string MetodoPago { get; set; } = "MercadoPago";
        public string? PaymentToken { get; set; } // Token de Mercado Pago
    }

    /// <summary>
    /// Response de pago procesado
    /// </summary>
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }
    }
}
