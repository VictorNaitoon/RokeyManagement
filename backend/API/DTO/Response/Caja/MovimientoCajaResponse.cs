namespace API.DTO.Response.Caja
{
    public class MovimientoCajaResponse
    {
        public int Id { get; set; }
        public int Id_caja { get; set; }
        public int Id_negocio { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public int Id_usuario { get; set; }
    }
}
