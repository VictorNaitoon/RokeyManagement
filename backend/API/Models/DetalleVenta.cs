using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("detalle_venta")]
    public class DetalleVenta
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdVenta { get; set; }
        [ForeignKey("IdVenta")]
        public virtual Venta Venta { get; set; } = null!;
        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; } = null!;
        public int Cantidad { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }
    }
}
