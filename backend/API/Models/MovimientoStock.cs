using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("movimiento_stock")]
    public class MovimientoStock
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; } = null!;
        [Required]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;
        public int? IdVenta { get; set; }
        [ForeignKey("IdVenta")]
        public virtual Venta? Venta { get; set; }
        public int? IdCompra { get; set; }
        [ForeignKey("IdCompra")]
        public virtual Compra? Compra { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public int Cantidad { get; set; }
        public Enums.TipoMovimiento TipoMovimiento { get; set; }
    }
}
