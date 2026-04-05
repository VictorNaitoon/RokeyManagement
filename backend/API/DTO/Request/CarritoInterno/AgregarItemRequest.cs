using System.ComponentModel.DataAnnotations;

namespace API.DTO.Request.CarritoInterno
{
    public class AgregarItemRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdProducto { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [MaxLength(500)]
        public string? Notas { get; set; }
    }
}
