using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("auditoria")]
    public class Auditoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la entidad auditada (Venta, Compra, Producto, Presupuesto, Usuario, Caja)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Entidad { get; set; } = string.Empty;

        /// <summary>
        /// ID del registro modificado
        /// </summary>
        [Required]
        public int IdRegistro { get; set; }

        /// <summary>
        /// ID del usuario que realizó la acción
        /// </summary>
        [Required]
        public int IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!;

        /// <summary>
        /// Tipo de acción: CREATE, UPDATE, SOFT_DELETE
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Accion { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de la acción
        /// </summary>
        [Required]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// JSON con el estado anterior (null en Create)
        /// </summary>
        public string? DatosAnteriores { get; set; }

        /// <summary>
        /// JSON con el estado nuevo (null en Delete)
        /// </summary>
        public string? DatosNuevos { get; set; }

        /// <summary>
        /// ID del negocio (multi-tenancy)
        /// </summary>
        [Required]
        public int Id_negocio { get; set; }

        [ForeignKey("Id_negocio")]
        public Negocio Negocio { get; set; } = null!;
    }
}