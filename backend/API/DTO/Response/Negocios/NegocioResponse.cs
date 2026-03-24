namespace API.DTO.Response.Negocios
{
    public class NegocioResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CUIT { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? LogoURL { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal IngresosBrutos { get; set; }
        public DateTime FechaInicioActividades { get; set; }
        public string? PuntoDeVenta { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? CondicionVentas { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalProductos { get; set; }
    }
}