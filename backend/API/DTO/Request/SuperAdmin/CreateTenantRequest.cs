namespace API.DTO.Request.SuperAdmin
{
    /// <summary>
    /// Request para que el Super Admin registre un nuevo negocio manualmente
    /// </summary>
    public class CreateTenantRequest
    {
        // Datos del usuario admin del negocio
        public string EmailAdmin { get; set; } = string.Empty;
        public string PasswordAdmin { get; set; } = string.Empty;
        public string NombreAdmin { get; set; } = string.Empty;
        public string ApellidoAdmin { get; set; } = string.Empty;

        // Datos del negocio
        public string Nombre { get; set; } = string.Empty;
        public string CUIT { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? LogoURL { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? PuntoDeVenta { get; set; }
        public string? CondicionVentas { get; set; }
        public int Tipo { get; set; } // 0=Cerrajeria, 1=Ferreteria, 2=MotoRepuestos, 3=AutoRepuestos
        public bool Activo { get; set; } = true;

        // Datos de la suscripción
        public int IdPlan { get; set; }
        public string TipoFacturacion { get; set; } = "Mensual"; // Mensual, Anual
        public bool ActivarSuscripcion { get; set; } = true; // Si es true, la suscripción queda activa (ya pagó)
    }
}