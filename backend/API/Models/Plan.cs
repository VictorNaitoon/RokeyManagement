using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("plan")]
    public class Plan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioMensual { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioAnual { get; set; }

        [Required]
        public int MaxUsuarios { get; set; }

        [Required]
        public int MaxProductos { get; set; }

        [Required]
        public int MaxTransaccionesMes { get; set; }

        public bool SoportePrioritario { get; set; }

        public bool MultiSucursal { get; set; }

        public bool APIAccess { get; set; }

        public bool Activo { get; set; } = true;

        public int Orden { get; set; } // Para ordering visual en UI

        // Relaciones
        public ICollection<Suscripcion> Suscripciones { get; set; } = new List<Suscripcion>();
    }
}
