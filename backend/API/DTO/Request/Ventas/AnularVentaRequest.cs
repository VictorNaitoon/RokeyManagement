using System.ComponentModel.DataAnnotations;

namespace API.DTO.Request.Ventas
{
    public class AnularVentaRequest
    {
        [MaxLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
        public string? Motivo { get; set; }
    }
}
