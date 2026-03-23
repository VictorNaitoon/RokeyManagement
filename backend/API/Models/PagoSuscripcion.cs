using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("pago_suscripcion")]
    public class PagoSuscripcion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int IdSuscripcion { get; set; }
        [ForeignKey("IdSuscripcion")]
        public Suscripcion Suscripcion { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        public DateTime FechaPago { get; set; }

        [Required]
        public Enums.MetodoPagoSuscripcion Metodo { get; set; }

        [MaxLength(200)]
        public string? TransactionId { get; set; }

        [Required]
        public Enums.EstadoPago Estado { get; set; }

        [MaxLength(500)]
        public string? Detalles { get; set; } // Para guardar info adicional del payment gateway
    }
}
