using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("proveedor")]
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        [Required]
        [MaxLength(200)]
        [Display(Name = "Nombre o Razón social del Proveedor")]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? Telefono { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }

        //Listas
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    }
}
