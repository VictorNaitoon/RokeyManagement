namespace API.DTO.Request.Negocios
{
    public class ActualizarNegocioRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string CUIT { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? LogoURL { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? PuntoDeVenta { get; set; }
        public string? CondicionVentas { get; set; }
        public int Tipo { get; set; } // 0=Cerrajeria, 1=Ferreteria, 2=MotoRepuestos, 3=AutoRepuestos
    }
}