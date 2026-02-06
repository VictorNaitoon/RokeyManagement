using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("categoria")]
    public class Categoria
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        [Required]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        //Listado de producto
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();

    }
}
