using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("detalle_presupuesto")]
    public class DetallePresupuesto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdPresupuesto { get; set; }
        [ForeignKey("IdPresupuesto")]
        public virtual Presupuesto Presupuesto { get; set; } = null!;
        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; } = null!;
        public int Cantidad { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioPactado { get; set; }
    }
}
