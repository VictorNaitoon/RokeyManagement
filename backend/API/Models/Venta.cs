using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("venta")]
    public class Venta
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        [Required]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public virtual Cliente? Cliente { get; set; }
        public DateTime FechaVenta { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalVenta { get; set; }

        //Relaciones y listas
        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
        public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>(); 
        public ICollection<Factura>? Facturas { get; set; } 
    }
}
