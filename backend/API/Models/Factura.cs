using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("factura")]
    public class Factura
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        [Required]
        public int IdVenta { get; set; }
        [ForeignKey("IdVenta")]
        public virtual Venta Venta { get; set; } = null!;
        [MaxLength(20)]
        public string? CuitCliente { get; set; }
        public DateTime FechaRealizada { get; set; }
        public Enums.TipoComprobante TipoFactura { get; set; }
        [MaxLength(50)]
        public string NumeroComprobante { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? CAE { get; set; }
        public DateTime? VencimientoCAE { get; set; }
        public string? QR { get; set; }
        [MaxLength(100)]
        public string? CondicionVenta { get; set; }
    }
}
