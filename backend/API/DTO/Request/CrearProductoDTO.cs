using System.ComponentModel.DataAnnotations;

namespace API.DTO.Request
{
    public class CrearProductoDTO
    {
        [Required]
        public int IdNegocio { get; set; }
        [Required]
        public int IdUsuarioCreador { get; set; }
        public int? IdCategoria { get; set; }
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

    }
}
