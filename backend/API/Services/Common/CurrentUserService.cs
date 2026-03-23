namespace API.Services.Common
{
    public interface ICurrentUserService
    {
        /// <summary>
        /// ID del usuario autenticado
        /// </summary>
        int UserId { get; }
        
        /// <summary>
        /// ID del negocio al que pertenece el usuario (multi-tenancy)
        /// </summary>
        int NegocioId { get; }
        
        /// <summary>
        /// Rol del usuario (Admin, Manager, Vendedor)
        /// </summary>
        string Rol { get; }
        
        /// <summary>
        /// Verifica si el usuario es Administrador
        /// </summary>
        bool IsAdmin => Rol == "Admin" || Rol == "Administrador";
        
        /// <summary>
        /// Verifica si el usuario es Manager
        /// </summary>
        bool IsManager => Rol == "Manager" || Rol == "Gerente";
        
        /// <summary>
        /// Verifica si el usuario es Vendedor
        /// </summary>
        bool IsVendedor => Rol == "Vendedor" || Rol == "Seller";
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value 
                    ?? _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                return int.TryParse(value, out var id) ? id : 0;
            }
        }

        public int NegocioId
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?.User.FindFirst("negocioId")?.Value;
                return int.TryParse(value, out var id) ? id : 0;
            }
        }

        public string Rol
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User.FindFirst("rol")?.Value ?? string.Empty;
            }
        }
    }
}