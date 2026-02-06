using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("presupuesto")]
    public class Presupuesto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Es para saber a que negocio pertenece el presupuesto linkeando la tabla "negocio" por IdNegocio. 
        /// Es importante para saber a qué negocio pertenece el presupuesto, ya que un mismo sistema puede ser utilizado por varios negocios diferentes, y cada presupuesto debe estar asociado al negocio correspondiente para mantener la organización y la integridad de los datos. 
        /// Además, permite filtrar y gestionar los presupuestos de manera eficiente según el negocio al que pertenecen.
        /// </summary>
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        /// <summary>
        /// Es para saber qué usuario creó el presupuesto, linkeando la tabla "usuario" por IdUsuario.
        /// Es importante para saber quién es el responsable de cada presupuesto para mantener la trazabilidad y la responsabilidad de las acciones dentro del sistema.
        /// </summary>
        [Required]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;
        /// <summary>
        /// Es para saber a qué cliente se le hizo el presupuesto, linkeando la tabla "cliente" por IdCliente.
        /// Es importante para saber a qué cliente se le hizo el presupuesto, ya que un mismo cliente puede tener varios presupuestos diferentes, y cada presupuesto debe estar asociado al cliente correspondiente para mantener la organización y la integridad de los datos.
        /// Puede ser null porque a veces se puede generar un presupuesto sin tener un cliente específico, por ejemplo, para mostrar un presupuesto general o para un cliente potencial que aún no está registrado en el sistema. En esos casos, el IdCliente sería null, lo que indica que el presupuesto no está asociado a ningún cliente en particular.
        /// </summary>
        public int? IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public virtual Cliente? Cliente { get; set; }
        /// <summary>
        /// Es para saber la fecha en que se generó el presupuesto. Se utiliza para llevar un registro de cuándo se creó cada presupuesto, lo que es importante para la gestión y el seguimiento de los presupuestos a lo largo del tiempo.
        /// </summary>
        public DateTime FechaEmision { get; set; }
        /// <summary>
        /// Es para saber la fecha en que vence el presupuesto. 
        /// Se usa para establecer un plazo de validez para cada presupuesto, lo que es importante para la gestión y el seguimiento de los presupuestos, 
        /// ya que después de la fecha de vencimiento, el presupuesto puede considerarse obsoleto o no válido.
        /// </summary>
        public DateTime FechaVencimiento { get; set; }
        /// <summary>
        /// Es para saber el estado del presupuesto, que puede ser "Pendiente", "Aceptado" o "Rechazado".
        /// </summary>
        [Required]
        public Enums.EstadoPresupuesto Estado { get; set; }
        /// <summary>
        /// Es para saber el total del presupuesto, que se calcula sumando el precio pactado de cada detalle del presupuesto multiplicado por la cantidad correspondiente.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPresupuesto { get; set; }

        // Relaciones y listas
        /// <summary>
        /// Es el listado de los detalles del presupuesto, que son los productos o servicios incluidos en el presupuesto, con su cantidad y precio pactado.
        /// </summary>
        public ICollection<DetallePresupuesto> DetallesPresupuesto { get; set; } = new List<DetallePresupuesto>();
    }
}
