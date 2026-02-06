using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("producto")]
    public class Producto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public Negocio Negocio { get; set; } = null!;
        public int IdUsuarioCreador { get; set; }
        [ForeignKey("IdUsuarioCreador")]
        public Usuario UsuarioCreador { get; set; } = null!;
        public int? IdCategoria { get; set; }
        [ForeignKey("IdCategoria")]
        public Categoria? Categoria { get; set; }
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? CodigoBusqueda { get; set; }
        [MaxLength(500)]
        public string? Descripcion { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioCompra { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string? ImagenURL { get; set; }
        public bool EsServicio { get; set; }
        public bool Activo { get; set; } = true;

        // Relaciones
        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
        public ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
        public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
        public ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();
        public ICollection<DetallePresupuesto> DetallesPresupuesto { get; set; } = new List<DetallePresupuesto>();
    }
}
