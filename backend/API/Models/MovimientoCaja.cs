using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("movimiento_caja")]
    public class MovimientoCaja
    {
        /// <summary>
        /// Identificador único del movimiento de caja. Se genera automáticamente al crear un nuevo movimiento en la base de datos.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Identificador de la caja a la que pertenece el movimiento. Es obligatorio porque todo movimiento debe estar asociado a una caja.
        /// </summary>
        [Required]
        [Column("IdCaja")]
        public int Id_caja { get; set; }
        [ForeignKey("Id_caja")]
        public virtual Caja Caja { get; set; } = null!;
        /// <summary>
        /// Identificador del negocio al que pertenece el movimiento. Es obligatorio para la隔离 de datos por negocio.
        /// </summary>
        [Required]
        [Column("IdNegocio")]
        public int Id_negocio { get; set; }
        [ForeignKey("Id_negocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        /// <summary>
        /// Tipo de movimiento: 'Ingreso' o 'Egreso'. Determina si el monto suma o resta del total de la caja.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Tipo { get; set; } = string.Empty;
        /// <summary>
        /// Monto del movimiento. Debe ser mayor a cero.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }
        /// <summary>
        /// Descripción opcional del movimiento. Puede contener información adicional como el motivo del egreso o la fuente del ingreso.
        /// </summary>
        [MaxLength(500)]
        public string? Descripcion { get; set; }
        /// <summary>
        /// Fecha y hora en que se registró el movimiento.
        /// </summary>
        [Required]
        public DateTime Fecha { get; set; }
        /// <summary>
        /// Identificador del usuario que registró el movimiento. Es importante para la auditoría.
        /// </summary>
        [Required]
        [Column("IdUsuario")]
        public int Id_usuario { get; set; }
        [ForeignKey("Id_usuario")]
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
