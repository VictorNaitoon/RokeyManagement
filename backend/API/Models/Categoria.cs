using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("categoria")]
    public class Categoria
    {
        /// <summary>
        /// Es el identificador único de la categoría, que se genera automáticamente al crear una nueva categoría en la base de datos.
        /// Se autoincrementa para garantizar que cada categoría tenga un identificador único y no se repita con otras categorías.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Es un campo obligatorio que almacena el identificador del negocio al que pertenece la categoría, que hace referencia a la tabla "negocio" a través de la clave foránea "IdNegocio".
        /// Ayuda a organizar y clasificar las categorías dentro del sistema, facilitando la gestión y organización de los datos.
        /// </summary>
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        /// <summary>
        /// Es un campo obligatorio que almacena el identificador del usuario que creó la categoría, que hace referencia a la tabla "usuario" a través de la clave foránea "IdUsuario".
        /// Permite rastrear quién fue el responsable de crear cada categoría, lo que es útil para fines de auditoría y gestión de usuarios dentro del sistema.
        /// </summary>
        [Required]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;
        /// <summary>
        /// Es un campo obligatorio que almacena el nombre de la categoría.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        /// <summary>
        /// Es opcional y almacena una breve descripción de la categoría.
        /// </summary>
        [MaxLength(500)]
        public string? Descripcion { get; set; }
        /// <summary>
        /// Es un campo que indica si la categoría está activa o inactiva. 
        /// Mas que nada para no eliminar definitivamente a una categoría, sino para desactivarla y que no se pueda asignar a productos ni utilizar en el sistema, pero que quede registrada en la base de datos por si se necesita reactivar o consultar información histórica relacionada con esa categoría.
        /// </summary>
        public bool Activo { get; set; } = true;

        //Listado de producto
        /// <summary>
        /// Es el listado de los productos que pertenecen a esta categoría.
        /// </summary>
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();

    }
}
