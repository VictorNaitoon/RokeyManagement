using System.ComponentModel.DataAnnotations;

namespace API.DTO.Request.CarritoInterno
{
    public class CreateCarritoInternoRequest
    {
        [MaxLength(100)]
        public string? Nombre { get; set; }
    }
}
