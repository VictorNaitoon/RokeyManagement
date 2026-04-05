namespace API.DTO.Response.Proveedores
{
    public class ProveedorResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public DateTime FechaAlta { get; set; }
    }

    public class ProveedorListResponse
    {
        public List<ProveedorResponse> Proveedores { get; set; } = new List<ProveedorResponse>();
        public int Total { get; set; }
    }
}