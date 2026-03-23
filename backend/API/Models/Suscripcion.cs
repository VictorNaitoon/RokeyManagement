using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("suscripcion")]
    public class Suscripcion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("NegocioId")]
        public int Id_negocio { get; set; }
        [ForeignKey("Id_negocio")]
        public Negocio Negocio { get; set; } = null!;

        [Required]
        public int IdPlan { get; set; }
        [ForeignKey("IdPlan")]
        public Plan Plan { get; set; } = null!;

        [Required]
        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public DateTime? FechaProximoPago { get; set; }

        [Required]
        public Enums.EstadoSuscripcion Estado { get; set; }

        [Required]
        public Enums.TipoFacturacion TipoFacturacion { get; set; }

        public DateTime? FechaCancelacion { get; set; }

        [MaxLength(500)]
        public string? MotivoCancelacion { get; set; }

        // Relaciones
        public ICollection<PagoSuscripcion> Pagos { get; set; } = new List<PagoSuscripcion>();
    }
}
