using Microsoft.AspNetCore.Mvc;
using API.DTO.Response;
using API.Services.Auth;
using API.Models;

namespace API.Controllers.Auth
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(IAuthService authService, IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Iniciar sesión como usuario del negocio (Admin, Gerente, Empleado)
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (usuario, error) = await _authService.AuthenticateAsync(request.Email, request.Password);

            if (error != null)
            {
                return Unauthorized(new ErrorResponse { Error = error });
            }

            if (usuario == null)
            {
                return Unauthorized(new ErrorResponse { Error = "Credenciales inválidas" });
            }

            var token = _jwtService.GenerateToken(usuario);

            return Ok(new AuthResponse
            {
                Token = token,
                Usuario = new UsuarioDto
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Rol = usuario.Rol.ToString(),
                    IdNegocio = usuario.Id_negocio
                }
            });
        }

        /// <summary>
        /// Iniciar sesión como Super Admin (gestor de la plataforma)
        /// </summary>
        [HttpPost("super-admin/login")]
        [ProducesResponseType(typeof(SuperAdminAuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginSuperAdmin([FromBody] LoginRequest request)
        {
            var (superAdmin, error) = await _authService.AuthenticateSuperAdminAsync(request.Email, request.Password);

            if (error != null)
            {
                return Unauthorized(new ErrorResponse { Error = error });
            }

            if (superAdmin == null)
            {
                return Unauthorized(new ErrorResponse { Error = "Credenciales inválidas" });
            }

            var token = _jwtService.GenerateSuperAdminToken(superAdmin);

            return Ok(new SuperAdminAuthResponse
            {
                Token = token,
                SuperAdmin = new SuperAdminDto
                {
                    Id = superAdmin.Id,
                    Email = superAdmin.Email,
                    Rol = superAdmin.Rol
                }
            });
        }

        /// <summary>
        /// Registrar un nuevo usuario en un negocio
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var (usuario, error) = await _authService.RegisterAsync(
                request.Email,
                request.Password,
                request.Nombre,
                request.Apellido,
                request.IdNegocio,
                request.Rol
            );

            if (error != null)
            {
                return BadRequest(new ErrorResponse { Error = error });
            }

            if (usuario == null)
            {
                return BadRequest(new ErrorResponse { Error = "Error al registrar usuario" });
            }

            var token = _jwtService.GenerateToken(usuario);

            return CreatedAtAction(nameof(Login), new AuthResponse
            {
                Token = token,
                Usuario = new UsuarioDto
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Rol = usuario.Rol.ToString(),
                    IdNegocio = usuario.Id_negocio
                }
            });
        }
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public int IdNegocio { get; set; }
        public Enums.RolUsuario Rol { get; set; }
    }
}
