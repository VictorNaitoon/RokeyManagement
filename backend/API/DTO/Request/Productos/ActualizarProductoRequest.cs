using System.ComponentModel.DataAnnotations;

namespace API.DTO.Request.Productos
{
    public class ActualizarProductoRequest
    {
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? CodigoBusqueda { get; set; }

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor a 0")]
        public decimal PrecioVenta { get; set; }

        // Solo Admin puede modificar PrecioCompra - el controller decidirá si lo incluye
        [Range(0, double.MaxValue)]
        public decimal? PrecioCompra { get; set; }

        [Range(0, int.MaxValue)]
        public int StockActual { get; set; }

        [Range(0, int.MaxValue)]
        public int StockMinimo { get; set; }

        public string? ImagenURL { get; set; }

        public bool EsServicio { get; set; }

        public bool Activo { get; set; }

        public int? IdCategoria { get; set; }
    }
}