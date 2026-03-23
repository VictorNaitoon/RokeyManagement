using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("metrica_uso")]
    public class MetricaUso
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("IdNegocio")]
        public int Id_negocio { get; set; }
        [ForeignKey("Id_negocio")]
        public Negocio Negocio { get; set; } = null!;

        [Required]
        public int Mes { get; set; }

        [Required]
        public int Anio { get; set; }

        public int TotalUsuarios { get; set; }

        public int TotalProductos { get; set; }

        public int TotalTransacciones { get; set; }

        public long AlmacenamientoBytes { get; set; }

        public int TotalAPICalls { get; set; }

        [Required]
        public DateTime UltimaActualizacion { get; set; }
    }
}
