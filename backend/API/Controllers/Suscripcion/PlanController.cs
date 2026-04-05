using API.DTO.Response.Suscripcion;
using API.Services.Suscripcion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Suscripcion
{
    [ApiController]
    [Route("api/v1/planes")]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlanController(IPlanService planService)
        {
            _planService = planService;
        }

        /// <summary>
        /// Obtiene todos los planes de suscripción activos (público)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<PlanSuscResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPlanes()
        {
            var planes = await _planService.GetAllPlanesAsync();
            return Ok(planes);
        }

        /// <summary>
        /// Obtiene los detalles de un plan específico (público)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PlanSuscResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlanById(int id)
        {
            var plan = await _planService.GetPlanByIdAsync(id);
            if (plan == null)
            {
                return NotFound(new { Error = "Plan no encontrado" });
            }
            return Ok(plan);
        }
    }
}
