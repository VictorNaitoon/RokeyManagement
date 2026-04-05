using System.Text;
using API.Jobs;
using API.Data;
using API.Services;
using API.Services.Auth;
using API.Services.Tenant;
using API.Services.SuperAdmin;
using API.Services.Usuarios;
using API.Services.Categorias;
using API.Services.Negocios;
using API.Services.Productos;
using API.Services.Ventas;
using API.Services.Presupuestos;
using API.Services.Proveedores;
using API.Services.Compras;
using API.Services.Common;
using API.Services.Caja;
using API.Services.Publicos;
using API.Services.Clientes;
using API.Services.Facturas;
using API.Services.Auditoria;
using API.Services.Informes;
using API.Services.CarritoInterno;
using API.Services.Suscripcion;
using API.Middleware;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT. Ejemplo: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Resolver conflictos de nombres de schemas duplicados
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

/// <summary>
/// Inyección de dependencias para el contexto de la base de datos, utilizando PostgreSQL como proveedor.
/// </summary>
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("WebApiDatabase"))
);

builder.Services.AddScoped<SeedService>();

// --- JWT Authentication ---
var jwtKey = builder.Configuration["Jwt:Key"] ?? "RoKeySuperSecretKey2026!@#$%^&*()";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RoKeyAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "RoKeyApp";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Mapear el claim "rol" al claim "role" para que funcione la autorización
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = "rol",  // Usar el claim "rol" como rol
            NameClaimType = "email"  // Usar el claim "email" como nombre
        };
    });

builder.Services.AddAuthorization();

// Data Protection para cookies del carrito público
builder.Services.AddDataProtection();

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache(); // Cache en memoria para estado de suscripción
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<API.Services.Auth.IAuthService, API.Services.Auth.AuthService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ISuperAdminService, SuperAdminService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<INegocioService, NegocioService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IPresupuestoService, PresupuestoService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<ICajaService, CajaService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();

// Public Services (Catálogo + Carrito)
builder.Services.AddScoped<ICatalogoPublicoService, CatalogoPublicoService>();
builder.Services.AddScoped<ICarritoPublicoService, CarritoPublicoService>();

// Cliente Services
builder.Services.AddScoped<IClienteService, ClienteService>();

// Auditoria Services
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

// Informes Services
builder.Services.AddScoped<IInformesService, InformesService>();

// CarritoInterno Services
builder.Services.AddScoped<ICarritoInternoService, CarritoInternoService>();

// CarritoInterno Cleanup Job (runs daily at 3 AM)
builder.Services.AddHostedService<CarritoInternoCleanupJob>();

// SaaS Subscription Services
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<ISuscripcionService, SuscripcionService>();
builder.Services.AddScoped<IPagoSuscripcionService, PagoSuscripcionService>();
builder.Services.AddScoped<IMetricaUsoService, MetricaUsoService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<API.Services.Suscripcion.IAuthService, API.Services.Suscripcion.AuthService>();

// Background Job: Subscription Expiration (runs daily at 3 AM UTC)
builder.Services.AddHostedService<SubscriptionExpirationService>();


// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.Productos.CrearProductoRequestValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.Productos.ActualizarProductoRequestValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.Categorias.CrearCategoriaRequestValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.Categorias.ActualizarCategoriaRequestValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.Ventas.CrearVentaValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.Presupuestos.CreatePresupuestoValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.Presupuestos.UpdatePresupuestoValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.Proveedores.CrearProveedorValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.Proveedores.ActualizarProveedorValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.Compras.CrearCompraValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.Facturas.CrearFacturaValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.Facturas.NotaCreditoValidator>());

// CarritoInterno Validators
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.CarritoInterno.CreateCarritoInternoRequestValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.CarritoInterno.AgregarItemRequestValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.CarritoInterno.UpdateItemRequestValidator>());
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<API.DTO.Request.CarritoInterno.ConvertirCarritoRequestValidator>());

var app = builder.Build();

// Configure the HTTP request pipeline.

// 1. Manejo global de excepciones
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"Error interno del servidor\"}");
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/// <summary>
/// Seed de datos iniciales (planes de suscripción y Super Admin)
/// </summary>
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();

    context.Database.Migrate();
    await seedService.SeedPlanesAsync();
    await seedService.SeedSuperAdminAsync();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseSubscriptionBlocking();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class public for testing
public partial class Program { }
