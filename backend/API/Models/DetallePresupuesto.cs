using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("detalle_presupuesto")]
    public class DetallePresupuesto
    {
        /// <summary>
        /// Es el identificador único del detalle del presupuesto, se genera automáticamente. 
        /// Se autoincrementa, lo que facilita la gestión y el seguimiento de los detalles dentro del sistema.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Es para saber a qué presupuesto pertenece el detalle del presupuesto, linkeando la tabla "presupuesto" por IdPresupuesto.
        /// </summary>
        [Required]
        public int IdPresupuesto { get; set; }
        [ForeignKey("IdPresupuesto")]
        public virtual Presupuesto Presupuesto { get; set; } = null!;
        /// <summary>
        /// Es para saber a qué producto se refiere el detalle del presupuesto, linkeando la tabla "producto" por IdProducto.
        /// Ademas, es importante para saber a qué producto se refiere cada detalle del presupuesto, ya que un mismo presupuesto puede incluir varios productos diferentes, y cada detalle debe estar asociado al producto correspondiente para mantener la organización y la integridad de los datos.
        /// </summary>
        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; } = null!;
        /// <summary>
        /// Es para saber la cantidad de productos o servicios incluidos en el detalle del presupuesto.
        /// </summary>
        public int Cantidad { get; set; }
        /// <summary>
        /// Es para saber el precio pactado para cada producto o servicio incluido en el detalle del presupuesto.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioPactado { get; set; }
    }
}
