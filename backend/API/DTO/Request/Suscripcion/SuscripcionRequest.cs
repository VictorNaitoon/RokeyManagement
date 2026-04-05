using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTO.Request.Suscripcion
{
    /// <summary>
    /// DTO para crear o actualizar una suscripción
    /// </summary>
    public class SuscripcionRequest
    {
        [Required]
        public int IdPlan { get; set; }

        [Required]
        public Enums.TipoFacturacion TipoFacturacion { get; set; }
    }
}
