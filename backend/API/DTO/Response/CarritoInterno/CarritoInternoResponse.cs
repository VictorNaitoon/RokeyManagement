using API.Models;

namespace API.DTO.Response.CarritoInterno
{
    public class CarritoInternoResponse
    {
        public int Id { get; set; }
        public int IdNegocio { get; set; }
        public int IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public Enums.EstadoCarritoInterno Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public List<CarritoInternoItemResponse> Items { get; set; } = new();
        
        // Calculados
        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        public int CantidadItems => Items.Sum(i => i.Cantidad);
    }

    public class CarritoInternoItemResponse
    {
        public int Id { get; set; }
        public int CarritoInternoId { get; set; }
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
        public string? Notas { get; set; }
    }
}
