using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("presupuesto")]
    public class Presupuesto
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
        public int? IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public virtual Cliente? Cliente { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        [Required]
        public Enums.EstadoPresupuesto Estado { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPresupuesto { get; set; }

        // Relaciones y listas
        public ICollection<DetallePresupuesto> DetallesPresupuesto { get; set; } = new List<DetallePresupuesto>();
    }
}
