namespace API.DTO.Response.Categorias
{
    public class CategoriaResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaAlta { get; set; }
    }

    public class CategoriaListResponse
    {
        public List<CategoriaResponse> Categorias { get; set; } = new();
        public int Total { get; set; }
    }
}