using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTO.Response;
using API.Services.Auth;
using API.Models;
using API.Services.Common;

namespace API.Controllers.Auth
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly ICurrentUserService _currentUser;

        public AuthController(
            IAuthService authService, 
            IJwtService jwtService,
            ICurrentUserService currentUser)
        {
            _authService = authService;
            _jwtService = jwtService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Opciones de cookie para el refresh token
        /// </summary>
        private CookieOptions GetRefreshTokenCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            };
        }

        /// <summary>
        /// Limpia la cookie de refresh token
        /// </summary>
        private void ClearRefreshTokenCookie()
        {
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
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

            // Revocar tokens anteriores (single session)
            await _authService.RevokeAllUserTokensAsync(usuario.Id);

            // Generar refresh token
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();
            var rawToken = await _authService.GenerateRefreshTokenAsync(
                usuario.Id, 
                RefreshTokenUserType.Usuario, 
                ipAddress, 
                userAgent
            );
            
            // Guardar el token raw en cookie
            Response.Cookies.Append("refreshToken", rawToken, GetRefreshTokenCookieOptions());

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
        /// Refrescar el access token usando el refresh token de la cookie
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RefreshResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh()
        {
            // Leer el refresh token de la cookie
            if (!Request.Cookies.TryGetValue("refreshToken", out var rawToken) || string.IsNullOrEmpty(rawToken))
            {
                return Unauthorized(new ErrorResponse { Error = "Refresh token not provided" });
            }

            // Validar el refresh token
            var userInfo = await _authService.ValidateRefreshTokenAsync(rawToken);
            if (userInfo == null)
            {
                return Unauthorized(new ErrorResponse { Error = "Invalid or expired refresh token" });
            }

            // Generar nuevo access token con el rol real del usuario
            var rol = userInfo.Value.rol ?? "Vendedor";
            var accessToken = _jwtService.GenerateTokenFromRefreshClaims(
                userInfo.Value.userId!.Value,
                userInfo.Value.email!,
                userInfo.Value.negocioId!.Value,
                rol
            );

            // Calcular expiresIn según el rol
            var expiresIn = rol == "Cliente" ? 86400 : 28800; // 24h o 8h en segundos

            return Ok(new RefreshResponse
            {
                AccessToken = accessToken,
                ExpiresIn = expiresIn,
                TokenType = "Bearer"
            });
        }

        /// <summary>
        /// Revocar el refresh token (logout)
        /// </summary>
        [HttpPost("revoke")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RevokeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Revoke()
        {
            // Leer el refresh token de la cookie
            if (!Request.Cookies.TryGetValue("refreshToken", out var rawToken) || string.IsNullOrEmpty(rawToken))
            {
                return Unauthorized(new ErrorResponse { Error = "Refresh token not provided" });
            }

            // Revocar el token
            var revoked = await _authService.RevokeRefreshTokenAsync(rawToken);
            if (!revoked)
            {
                return Unauthorized(new ErrorResponse { Error = "Invalid or already revoked token" });
            }

            // Limpiar la cookie
            ClearRefreshTokenCookie();

            return Ok(new RevokeResponse { Message = "Token revoked successfully" });
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
        /// Registrar un nuevo usuario en el negocio (solo el Administrador puede hacerlo)
        /// </summary>
        [HttpPost("register")]
        [Authorize]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Verificar que el usuario actual sea Admin
            if (!_currentUser.IsAdmin)
            {
                return StatusCode(403, new ErrorResponse { Error = "Solo el administrador puede registrar nuevos usuarios" });
            }

            var (usuario, error) = await _authService.RegisterAsync(
                request.Email,
                request.Password,
                request.Nombre,
                request.Apellido,
                _currentUser.NegocioId, // Usar el negocio del usuario actual
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
        public Enums.RolUsuario Rol { get; set; }
    }
}
