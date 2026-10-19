using ECommerceJuegos.Models;
using Microsoft.AspNetCore.Identity;
using ECommerceJuegos.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// servicios MVC y API
builder.Services.AddControllersWithViews();

// Inyección de dependencias
builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddScoped<IRepositorioCategoria, RepositorioCategoria>();
builder.Services.AddScoped<IRepositorioProducto, RepositorioProducto>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IRepositorioClaveDigital, RepositorioClaveDigital>();
builder.Services.AddScoped<IRepositorioCompra, RepositorioCompra>();
builder.Services.AddScoped<IRepositorioResena, RepositorioResena>();
builder.Services.AddScoped<IRepositorioOrden, RepositorioOrden>();

// Configurar Autenticación
// La clave se configura externamente (User Secrets en desarrollo).
var jwtSecret = builder.Configuration["TokenAuthentication:SecretKey"];
var jwtIssuer = builder.Configuration["TokenAuthentication:Issuer"];
var jwtAudience = builder.Configuration["TokenAuthentication:Audience"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("Falta configurar TokenAuthentication:SecretKey externamente.");
if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException("Falta configurar TokenAuthentication:Issuer.");
if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("Falta configurar TokenAuthentication:Audience.");
var jwtKeyBytes = System.Text.Encoding.UTF8.GetBytes(jwtSecret);
if (jwtKeyBytes.Length < 32)
    throw new InvalidOperationException("TokenAuthentication:SecretKey debe tener al menos 32 bytes para HS256.");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.LogoutPath = "/Usuarios/Logout";
        options.AccessDeniedPath = "/Home/Restringido";
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            ClockSkew = TimeSpan.Zero
        };
    });

// autorizaciones por roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Administrador"));
});

var app = builder.Build();

// pipeline http (middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
