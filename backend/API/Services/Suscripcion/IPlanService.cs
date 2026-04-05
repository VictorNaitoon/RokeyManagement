using API.DTO.Response.Suscripcion;

namespace API.Services.Suscripcion
{
    /// <summary>
    /// Servicio para consultar los planes de suscripción disponibles
    /// </summary>
    public interface IPlanService
    {
        /// <summary>
        /// Obtiene todos los planes activos disponibles
        /// </summary>
        Task<List<PlanSuscResponse>> GetAllPlanesAsync(CancellationToken ct = default);

        /// <summary>
        /// Obtiene un plan específico por su ID
        /// </summary>
        Task<PlanSuscResponse?> GetPlanByIdAsync(int id, CancellationToken ct = default);
    }
}
