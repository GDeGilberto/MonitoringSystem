using Application.Interfaces;
using Application.UseCases;
using BlazorDateRangePicker;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Models;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Components;
using Web.Components.Account;

var builder = WebApplication.CreateBuilder(args);

// Configuration EF
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Configuración completa de Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    // Configuración de contraseñas
    options.Password.RequireDigit = true;                     // Requiere al menos un número
    options.Password.RequireLowercase = true;                 // Requiere al menos una minúscula
    options.Password.RequireUppercase = true;                 // Requiere al menos una mayúscula
    options.Password.RequireNonAlphanumeric = true;           // Requiere al menos un caracter especial
    options.Password.RequiredLength = 8;                      // Longitud mínima de 8 caracteres
    options.Password.RequiredUniqueChars = 1;                 // Requiere caracteres únicos

    // Configuración de bloqueo de cuenta
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);  // Duración del bloqueo: 15 minutos
    options.Lockout.MaxFailedAccessAttempts = 5;                        // Intentos fallidos antes de bloqueo
    options.Lockout.AllowedForNewUsers = true;                          // Habilitar bloqueo para nuevos usuarios

    // Configuración del usuario
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Caracteres permitidos en nombres de usuario
    options.User.RequireUniqueEmail = true;                             // Correos electrónicos deben ser únicos

    // Configuración de inicio de sesión
    options.SignIn.RequireConfirmedEmail = true;                        // Requiere correo confirmado para iniciar sesión
    options.SignIn.RequireConfirmedPhoneNumber = false;                 // No requiere teléfono confirmado
    options.SignIn.RequireConfirmedAccount = true;                      // Requiere confirmación de cuenta

    // Configuración de tokens
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
});

// Configurar opciones de cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;                           // La cookie solo es accesible por HTTP (no JavaScript)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Requiere HTTPS para enviar la cookie
    options.ExpireTimeSpan = TimeSpan.FromDays(14);           // Duración de la cookie
    options.SlidingExpiration = true;                         // Renovar el tiempo de expiración con cada solicitud
    options.LoginPath = "/Account/Login";                     // Ruta de inicio de sesión
    options.AccessDeniedPath = "/Account/AccessDenied";       // Ruta de acceso denegado
    options.LogoutPath = "/Account/Logout";                   // Ruta de cierre de sesión
});

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddDateRangePicker(config =>
{
    config.Attributes = new Dictionary<string, object>
    {
        {"class","form-control form-control-sm" }
    };
});

builder.Services.AddScoped<IRepositorySearch<ProcDescargaModel, DescargasEntity>, DescargasRepository>();
builder.Services.AddScoped<IRepository<EstacionesEntity>, EstacionesRepository>();
builder.Services.AddScoped<IRepository<InventarioEntity>, InventarioRepository>();
builder.Services.AddScoped<GetDescargaSearchUseCase<ProcDescargaModel>>();
builder.Services.AddScoped<GetLatestInventarioByStationUseCase<ProcInventarioModel>>();
builder.Services.AddScoped<GetEstacionesByIdUseCase>();

builder.Services.AddScoped<IRepositorySearch<ProcInventarioModel, InventarioEntity>, InventarioRepository>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();


// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
