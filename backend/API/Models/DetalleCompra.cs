using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("detalle_compra")]
    public class DetalleCompra
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdCompra { get; set; }
        [ForeignKey("IdCompra")]
        public virtual Compra Compra { get; set; } = null!;
        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; } = null!;
        public int Cantidad { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }
    }
}
