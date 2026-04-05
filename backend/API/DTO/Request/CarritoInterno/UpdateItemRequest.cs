using System.ComponentModel.DataAnnotations;

namespace API.DTO.Request.CarritoInterno
{
    public class UpdateItemRequest
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int Cantidad { get; set; }
    }
}
