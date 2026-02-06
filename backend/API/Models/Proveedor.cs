using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("proveedor")]
    public class Proveedor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   
        public int Id { get; set; }
        /// <summary>
        /// Relación con Negocio: Un proveedor pertenece a un negocio. 
        /// Esto permite que un mismo proveedor pueda estar asociado a múltiples negocios si es necesario, 
        /// y cada negocio puede tener su propia lista de proveedores.
        /// </summary>
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        /// <summary>
        /// Es un campo obligatorio que almacena el nombre del proveedor o su razón social.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Display(Name = "Nombre o Razón social del Proveedor")]
        public string Nombre { get; set; } = string.Empty;
        /// <summary>
        /// Es un campo opcional que puede almacenar el teléfono del proveedor.
        /// </summary>
        [MaxLength(50)]
        public string? Telefono { get; set; }
        /// <summary>
        /// Es un campo opcional que puede almacenar el correo electrónico del proveedor.
        /// </summary>
        [MaxLength(100)]
        public string? Email { get; set; }

        //Listas
        /// <summary>
        /// Relación con Compra: Un proveedor puede tener múltiples compras asociadas. 
        /// Que son las compras realizadas a ese proveedor. Esto permite rastrear el historial de compras y los gastos asociados a cada proveedor, lo cual es fundamental para la gestión financiera y el control de inventarios.
        /// </summary>
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    }
}
