using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("caja")]
    public class Caja
    {
        /// <summary>
        /// Identificador único de la caja. Se genera automáticamente al crear una nueva caja en la base de datos.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Identificador del negocio al que pertenece la caja. Es obligatorio porque cada caja debe estar asociada a un negocio específico.
        /// </summary>
        [Required]
        [Column("IdNegocio")]
        public int Id_negocio { get; set; }
        [ForeignKey("Id_negocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        /// <summary>
        /// Identificador del usuario que abrió la caja. Es obligatorio registrar quién realiza la apertura.
        /// </summary>
        [Required]
        [Column("IdUsuarioApertura")]
        public int Id_usuario_apertura { get; set; }
        [ForeignKey("Id_usuario_apertura")]
        public virtual Usuario UsuarioApertura { get; set; } = null!;
        /// <summary>
        /// Identificador del usuario que cerró la caja. Es opcional porque la caja puede estar abierta.
        /// </summary>
        [Column("IdUsuarioCierre")]
        public int? Id_usuario_cierre { get; set; }
        [ForeignKey("Id_usuario_cierre")]
        public virtual Usuario? UsuarioCierre { get; set; }
        /// <summary>
        /// Fecha y hora en que se abrió la caja. Es importante para el control y auditoría de las operaciones de caja.
        /// </summary>
        [Required]
        public DateTime FechaApertura { get; set; }
        /// <summary>
        /// Fecha y hora en que se cerró la caja. Es opcional porque la caja puede estar abierta.
        /// </summary>
        public DateTime? FechaCierre { get; set; }
        /// <summary>
        /// Monto inicial con el que se abre la caja. Es obligatorio y debe ser mayor o igual a cero.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoInicial { get; set; }
        /// <summary>
        /// Monto final con el que se cierra la caja. Es opcional porque la caja puede estar abierta.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontoFinal { get; set; }
        /// <summary>
        /// Estado de la caja: 'Abierta' o 'Cerrada'. Solo puede haber una caja abierta por negocio a la vez.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Abierta";

        // Listas
        /// <summary>
        /// Una caja puede tener múltiples movimientos asociados (ingresos y egresos).
        /// </summary>
        public ICollection<MovimientoCaja> Movimientos { get; set; } = new List<MovimientoCaja>();
    }
}
