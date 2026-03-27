using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using API.Models;

namespace API.Services.Auth
{
    public interface IJwtService
    {
        string GenerateToken(Usuario usuario);
        string GenerateSuperAdminToken(Models.SuperAdmin superAdmin);
        string GenerateClienteToken(Cliente cliente);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "RoKeySuperSecretKey2026!@#$%^&*()")
            );
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim("negocioId", usuario.Id_negocio.ToString()),
                new Claim("rol", usuario.Rol.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "RoKeyAPI",
                audience: _configuration["Jwt:Audience"] ?? "RoKeyApp",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateSuperAdminToken(Models.SuperAdmin superAdmin)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "RoKeySuperSecretKey2026!@#$%^&*()")
            );
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, superAdmin.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, superAdmin.Email),
                new Claim("rol", superAdmin.Rol),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "RoKeyAPI",
                audience: _configuration["Jwt:Audience"] ?? "RoKeyApp",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateClienteToken(Cliente cliente)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "RoKeySuperSecretKey2026!@#$%^&*()")
            );
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, cliente.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, cliente.Email ?? ""),
                new Claim("negocioId", cliente.Id_negocio.ToString()),
                new Claim("rol", "Cliente"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "RoKeyAPI",
                audience: _configuration["Jwt:Audience"] ?? "RoKeyApp",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24), // 24 horas para clientes
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
