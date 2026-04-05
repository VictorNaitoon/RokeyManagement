using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Models;
using API.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace API.Tests.Auth;

public class RefreshTokenTests
{
    /// <summary>
    /// Test 6.1: Unit tests for RefreshToken entity validation
    /// Validates that the RefreshToken entity is created correctly with all required properties.
    /// </summary>
    [Fact]
    public void RefreshToken_EntityCreation_ShouldHaveAllRequiredProperties()
    {
        // Arrange & Act
        var refreshToken = new RefreshToken
        {
            Id = 1,
            Token = "test-token-hash",
            UserId = 100,
            UserType = RefreshTokenUserType.Usuario,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            RevokedAt = null,
            CreatedAt = DateTime.UtcNow,
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0 Test"
        };

        // Assert
        Assert.Equal(1, refreshToken.Id);
        Assert.Equal("test-token-hash", refreshToken.Token);
        Assert.Equal(100, refreshToken.UserId);
        Assert.Equal(RefreshTokenUserType.Usuario, refreshToken.UserType);
        Assert.True(refreshToken.ExpiresAt > DateTime.UtcNow);
        Assert.Null(refreshToken.RevokedAt);
        Assert.NotNull(refreshToken.CreatedAt);
        Assert.Equal("192.168.1.1", refreshToken.IpAddress);
        Assert.Equal("Mozilla/5.0 Test", refreshToken.UserAgent);
    }

    [Theory]
    [InlineData(RefreshTokenUserType.Usuario)]
    [InlineData(RefreshTokenUserType.SuperAdmin)]
    [InlineData(RefreshTokenUserType.Cliente)]
    public void RefreshToken_UserType_ShouldAcceptAllValidTypes(RefreshTokenUserType userType)
    {
        // Arrange & Act
        var refreshToken = new RefreshToken
        {
            Token = "test-token",
            UserId = 1,
            UserType = userType,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(userType, refreshToken.UserType);
    }

    [Fact]
    public void RefreshToken_RevokedAt_ShouldBeNullable()
    {
        // Arrange & Act
        var refreshToken = new RefreshToken
        {
            Token = "test-token",
            UserId = 1,
            UserType = RefreshTokenUserType.Usuario,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };

        // Assert - Initially null when active
        Assert.Null(refreshToken.RevokedAt);

        // Act - Set revocation date
        refreshToken.RevokedAt = DateTime.UtcNow;

        // Assert - Now has a value
        Assert.NotNull(refreshToken.RevokedAt);
        Assert.True(refreshToken.RevokedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void RefreshToken_DefaultValues_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var refreshToken = new RefreshToken();

        // Assert
        Assert.Equal(0, refreshToken.Id);
        Assert.Equal(0, refreshToken.UserId);
        Assert.Equal(string.Empty, refreshToken.Token);
        Assert.Null(refreshToken.RevokedAt);
        Assert.Null(refreshToken.IpAddress);
        Assert.Null(refreshToken.UserAgent);
    }
}

public class JwtServiceTests
{
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public JwtServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?> {
            {"Jwt:Key", "RoKeySuperSecretKey2026!@#$%^&*()"},
            {"Jwt:Issuer", "RoKeyAPI"},
            {"Jwt:Audience", "RoKeyApp"}
        };
        
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
            
        _jwtService = new JwtService(_configuration);
    }

    /// <summary>
    /// Test 6.2: Unit tests for JwtService.GenerateTokenFromRefreshClaims
    /// Validates token generation from refresh claims.
    /// </summary>
    [Fact]
    public void GenerateTokenFromRefreshClaims_WithUsuarioRole_ShouldGenerate8HourToken()
    {
        // Arrange
        var userId = 1;
        var email = "test@test.com";
        var negocioId = 100;
        var rol = "Usuario";

        // Act
        var token = _jwtService.GenerateTokenFromRefreshClaims(userId, email, negocioId, rol);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Validate token structure
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        Assert.Equal(userId.ToString(), jwtToken.Subject);
        Assert.Equal(email, jwtToken.Claims.First(c => c.Type == "email").Value);
        Assert.Equal(negocioId.ToString(), jwtToken.Claims.First(c => c.Type == "negocioId").Value);
        Assert.Equal(rol, jwtToken.Claims.First(c => c.Type == "rol").Value);
        
        // Verify 8-hour expiration for Usuario
        var expectedExpiry = DateTime.UtcNow.AddHours(8);
        Assert.True(Math.Abs((jwtToken.ValidTo - expectedExpiry).TotalMinutes) < 1);
    }

    [Fact]
    public void GenerateTokenFromRefreshClaims_WithClienteRole_ShouldGenerate24HourToken()
    {
        // Arrange
        var userId = 1;
        var email = "cliente@test.com";
        var negocioId = 100;
        var rol = "Cliente";

        // Act
        var token = _jwtService.GenerateTokenFromRefreshClaims(userId, email, negocioId, rol);

        // Assert
        Assert.NotNull(token);
        
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        // Verify 24-hour expiration for Cliente
        var expectedExpiry = DateTime.UtcNow.AddHours(24);
        Assert.True(Math.Abs((jwtToken.ValidTo - expectedExpiry).TotalMinutes) < 1);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Gerente")]
    [InlineData("Empleado")]
    [InlineData("SuperAdmin")]
    public void GenerateTokenFromRefreshClaims_WithDifferentRoles_ShouldGenerate8HourToken(string rol)
    {
        // Arrange
        var userId = 1;
        var email = "test@test.com";
        var negocioId = 100;

        // Act
        var token = _jwtService.GenerateTokenFromRefreshClaims(userId, email, negocioId, rol);

        // Assert
        Assert.NotNull(token);
        
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        // Verify 8-hour expiration (default for non-Cliente roles)
        var expectedExpiry = DateTime.UtcNow.AddHours(8);
        Assert.True(Math.Abs((jwtToken.ValidTo - expectedExpiry).TotalMinutes) < 1);
    }

    [Fact]
    public void GenerateTokenFromRefreshClaims_ShouldContainAllRequiredClaims()
    {
        // Arrange
        var userId = 42;
        var email = "user@example.com";
        var negocioId = 999;
        var rol = "Admin";

        // Act
        var token = _jwtService.GenerateTokenFromRefreshClaims(userId, email, negocioId, rol);

        // Assert
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        Assert.Contains(jwtToken.Claims, c => c.Type == "sub");
        Assert.Contains(jwtToken.Claims, c => c.Type == "email");
        Assert.Contains(jwtToken.Claims, c => c.Type == "negocioId");
        Assert.Contains(jwtToken.Claims, c => c.Type == "rol");
        Assert.Contains(jwtToken.Claims, c => c.Type == "jti");
    }
}

public class AuthServiceRefreshTokenTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    /// <summary>
    /// Test 6.3: Unit tests for AuthService refresh token methods
    /// Tests: GenerateRefreshTokenAsync, ValidateRefreshTokenAsync, RevokeRefreshTokenAsync, RevokeAllUserTokensAsync
    /// </summary>
    
    // GenerateRefreshTokenAsync Tests
    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldCreateValidToken()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);
        
        // Create test user
        var usuario = new Usuario
        {
            Id = 1,
            Email = "test@test.com",
            Id_negocio = 100,
            Activo = true
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        // Act
        var rawToken = await authService.GenerateRefreshTokenAsync(
            userId: 1,
            userType: RefreshTokenUserType.Usuario,
            ipAddress: "192.168.1.1",
            userAgent: "Test Agent"
        );

        // Assert
        Assert.NotNull(rawToken);
        Assert.NotEmpty(rawToken);
        Assert.Contains(":", rawToken); // Should contain GUID:entropy format
        
        // Verify token was saved to DB (as hash)
        var storedToken = context.RefreshTokens.FirstOrDefault();
        Assert.NotNull(storedToken);
        Assert.Equal(1, storedToken.UserId);
        Assert.Equal(RefreshTokenUserType.Usuario, storedToken.UserType);
        Assert.NotNull(storedToken.Token); // Should be hashed
        Assert.True(storedToken.ExpiresAt > DateTime.UtcNow);
        Assert.Null(storedToken.RevokedAt);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldExpireIn30Days()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);
        
        var usuario = new Usuario { Id = 1, Email = "test@test.com", Id_negocio = 100, Activo = true };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        // Act
        var rawToken = await authService.GenerateRefreshTokenAsync(
            userId: 1,
            userType: RefreshTokenUserType.Usuario,
            ipAddress: null,
            userAgent: null
        );

        // Assert
        var storedToken = context.RefreshTokens.First();
        var expectedExpiry = DateTime.UtcNow.AddDays(30);
        Assert.True(Math.Abs((storedToken.ExpiresAt - expectedExpiry).TotalMinutes) < 1);
    }

    // ValidateRefreshTokenAsync Tests
    [Fact]
    public async Task ValidateRefreshTokenAsync_WithValidToken_ShouldReturnUserInfo()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);
        
        var usuario = new Usuario { Id = 1, Email = "test@test.com", Id_negocio = 100, Activo = true };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        
        var rawToken = await authService.GenerateRefreshTokenAsync(1, RefreshTokenUserType.Usuario, "1.1.1.1", "Agent");

        // Act
        var result = await authService.ValidateRefreshTokenAsync(rawToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Value.userId);
        Assert.Equal(RefreshTokenUserType.Usuario, result.Value.userType);
        Assert.Equal(100, result.Value.negocioId);
        Assert.Equal("test@test.com", result.Value.email);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithNonExistentToken_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);

        // Act
        var result = await authService.ValidateRefreshTokenAsync("non-existent-token");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithRevokedToken_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);
        
        var usuario = new Usuario { Id = 1, Email = "test@test.com", Id_negocio = 100, Activo = true };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        
        var rawToken = await authService.GenerateRefreshTokenAsync(1, RefreshTokenUserType.Usuario, "1.1.1.1", "Agent");
        
        // Revoke the token
        await authService.RevokeRefreshTokenAsync(rawToken);

        // Act
        var result = await authService.ValidateRefreshTokenAsync(rawToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        // Create expired token directly in DB
        var refreshToken = new RefreshToken
        {
            Token = "expired-token-hash",
            UserId = 1,
            UserType = RefreshTokenUserType.Usuario,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Already expired
            CreatedAt = DateTime.UtcNow.AddDays(-31),
            RevokedAt = null
        };
        context.RefreshTokens.Add(refreshToken);
        
        var usuario = new Usuario { Id = 1, Email = "test@test.com", Id_negocio = 100, Activo = true };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        
        var authService = new AuthService(context);

        // Act - Use raw token (this will hash to "expired-token-hash" if it was the raw value)
        // But we can't predict the hash, so let's test with a non-matching approach
        // We'll create a manual token that we can verify
        var rawToken = "test:expired";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        var tokenHash = Convert.ToBase64String(hashedBytes);
        
        refreshToken.Token = tokenHash;
        await context.SaveChangesAsync();
        
        var result = await authService.ValidateRefreshTokenAsync(rawToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithNullToken_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);

        // Act
        var result = await authService.ValidateRefreshTokenAsync(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithEmptyToken_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);

        // Act
        var result = await authService.ValidateRefreshTokenAsync("");

        // Assert
        Assert.Null(result);
    }

    // RevokeRefreshTokenAsync Tests
    [Fact]
    public async Task RevokeRefreshTokenAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);
        
        var usuario = new Usuario { Id = 1, Email = "test@test.com", Id_negocio = 100, Activo = true };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        
        var rawToken = await authService.GenerateRefreshTokenAsync(1, RefreshTokenUserType.Usuario, "1.1.1.1", "Agent");

        // Act
        var result = await authService.RevokeRefreshTokenAsync(rawToken);

        // Assert
        Assert.True(result);
        
        // Verify token is revoked in DB
        var storedToken = context.RefreshTokens.First();
        Assert.NotNull(storedToken.RevokedAt);
        Assert.True(storedToken.RevokedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithAlreadyRevokedToken_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);
        
        var usuario = new Usuario { Id = 1, Email = "test@test.com", Id_negocio = 100, Activo = true };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        
        var rawToken = await authService.GenerateRefreshTokenAsync(1, RefreshTokenUserType.Usuario, "1.1.1.1", "Agent");
        await authService.RevokeRefreshTokenAsync(rawToken); // First revocation

        // Act
        var result = await authService.RevokeRefreshTokenAsync(rawToken); // Second revocation

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithNonExistentToken_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);

        // Act
        var result = await authService.RevokeRefreshTokenAsync("non-existent-token");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithNullToken_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);

        // Act
        var result = await authService.RevokeRefreshTokenAsync(null!);

        // Assert
        Assert.False(result);
    }

    // RevokeAllUserTokensAsync Tests
    [Fact]
    public async Task RevokeAllUserTokensAsync_WithActiveTokens_ShouldRevokeAllAndReturnCount()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);
        
        var usuario = new Usuario { Id = 1, Email = "test@test.com", Id_negocio = 100, Activo = true };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        
        // Generate multiple tokens
        var token1 = await authService.GenerateRefreshTokenAsync(1, RefreshTokenUserType.Usuario, "1.1.1.1", "Agent");
        var token2 = await authService.GenerateRefreshTokenAsync(1, RefreshTokenUserType.Usuario, "1.1.1.2", "Agent");
        var token3 = await authService.GenerateRefreshTokenAsync(1, RefreshTokenUserType.Usuario, "1.1.1.3", "Agent");

        // Act
        var revokedCount = await authService.RevokeAllUserTokensAsync(1);

        // Assert
        Assert.Equal(3, revokedCount);
        
        // Verify all tokens are revoked
        var activeTokens = context.RefreshTokens.Where(rt => rt.UserId == 1 && rt.RevokedAt == null).ToList();
        Assert.Empty(activeTokens);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithNoActiveTokens_ShouldReturnZero()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);
        
        var usuario = new Usuario { Id = 1, Email = "test@test.com", Id_negocio = 100, Activo = true };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        
        // Generate and revoke tokens
        var rawToken = await authService.GenerateRefreshTokenAsync(1, RefreshTokenUserType.Usuario, "1.1.1.1", "Agent");
        await authService.RevokeRefreshTokenAsync(rawToken);

        // Act
        var revokedCount = await authService.RevokeAllUserTokensAsync(1);

        // Assert
        Assert.Equal(0, revokedCount);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithDifferentUserTokens_ShouldOnlyRevokeTargetUserTokens()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);
        
        var usuario1 = new Usuario { Id = 1, Email = "user1@test.com", Id_negocio = 100, Activo = true };
        var usuario2 = new Usuario { Id = 2, Email = "user2@test.com", Id_negocio = 100, Activo = true };
        context.Usuarios.AddRange(usuario1, usuario2);
        await context.SaveChangesAsync();
        
        // Generate tokens for both users
        await authService.GenerateRefreshTokenAsync(1, RefreshTokenUserType.Usuario, "1.1.1.1", "Agent");
        await authService.GenerateRefreshTokenAsync(2, RefreshTokenUserType.Usuario, "2.2.2.2", "Agent");

        // Act
        var revokedCount = await authService.RevokeAllUserTokensAsync(1);

        // Assert
        Assert.Equal(1, revokedCount);
        
        // Verify only user1's tokens are revoked
        var user1Tokens = context.RefreshTokens.Where(rt => rt.UserId == 1 && rt.RevokedAt == null).ToList();
        var user2Tokens = context.RefreshTokens.Where(rt => rt.UserId == 2 && rt.RevokedAt == null).ToList();
        
        Assert.Empty(user1Tokens);
        Assert.Single(user2Tokens);
    }

    // GetUserInfoAsync Tests
    [Fact]
    public async Task GetUserInfoAsync_WithUsuario_ShouldReturnEmailAndNegocioId()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);
        
        var usuario = new Usuario 
        { 
            Id = 1, 
            Email = "test@test.com", 
            Id_negocio = 100, 
            Activo = true,
            PasswordHash = "hash",
            Nombre = "Test",
            Apellido = "User"
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        // Act
        var result = await authService.GetUserInfoAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Value.email);
        Assert.Equal(100, result.Value.negocioId);
    }

    [Fact]
    public async Task GetUserInfoAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);

        // Act
        var result = await authService.GetUserInfoAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserInfoAsync_WithSuperAdmin_ShouldReturnEmailAndZeroNegocioId()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authService = new AuthService(context);
        
        var superAdmin = new Models.SuperAdmin
        {
            Id = 1,
            Email = "super@admin.com",
            Activo = true,
            PasswordHash = "hash",
            Rol = "SuperAdmin"
        };
        context.SuperAdmins.Add(superAdmin);
        await context.SaveChangesAsync();

        // Act
        var result = await authService.GetUserInfoAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("super@admin.com", result.Value.email);
        Assert.Equal(0, result.Value.negocioId); // SuperAdmin has negocioId = 0
    }
}
