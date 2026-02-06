using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("compra")]
    public class Compra
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        [Required]
        public int IdProveedor { get; set; }
        [ForeignKey("IdProveedor")]
        public virtual Proveedor Proveedor { get; set; } = null!;
        [Required]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;
        public DateTime FechaCompra { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGasto { get; set; }
        [MaxLength(50)]
        public string? NumeroComprobante { get; set; }

        // Listas
        public ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
        public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
    }
}
