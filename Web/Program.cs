using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using BlazorDateRangePicker;
using Domain.Entities;
using Infrastructure.Communication;
using Infrastructure.Data;
using Infrastructure.Jobs;
using Infrastructure.Models;
using Infrastructure.Presenters;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Components;
using Web.Components.Account;
using Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

var isDemoMode = builder.Environment.EnvironmentName == "Demo";

Console.WriteLine($"Web (Blazor) ejecutándose en el environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Modo Demo: {(isDemoMode ? "ACTIVADO" : "DESACTIVADO")}");

if (isDemoMode)
{
    // Configure demo services
    builder.Services.AddDemoServices(builder.Configuration);
    Console.WriteLine("? Servicios de demostración configurados");
}
else
{
    // Configuration EF for production
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Repository registrations for production
    builder.Services.AddScoped<IRepositorySearch<ProcDescargaModel, DescargasEntity>, DescargasRepository>();
    builder.Services.AddScoped<IRepository<EstacionesEntity>, EstacionesRepository>();
    builder.Services.AddScoped<IRepository<InventarioEntity>, InventarioRepository>();
    builder.Services.AddScoped<IRepository<DescargasEntity>, DescargasRepository>();
    builder.Services.AddScoped<IRepository<TanqueEntity>, TanqueRepository>();
    builder.Services.AddScoped<ITanqueRepository, TanqueRepository>();
    builder.Services.AddScoped<IRepositorySearch<ProcInventarioModel, InventarioEntity>, InventarioRepository>();

    // Use Case registrations
    builder.Services.AddScoped<GetDescargaSearchUseCase<ProcDescargaModel>>();
    builder.Services.AddScoped<GetLatestInventarioByStationUseCase<ProcInventarioModel>>();
    builder.Services.AddScoped<GetEstacionesByIdUseCase>();
    builder.Services.AddScoped<GetTanqueByEstacionAndNumeroUseCase>();

    // Serial Port Services for production
    builder.Services.AddSingleton<ISerialPortService, SerialPortManager>();

    // Application Services
    builder.Services.AddScoped<DescargasService<DescargasEntity>>();
    builder.Services.AddScoped<InventarioService<InventarioEntity, InventarioViewModel>>();

    // SOAP Service for Dagal (production)
    builder.Services.AddHttpClient<IDagalSoapService, DagalSoapService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("User-Agent", "MonitoringSystem/1.0");
    });

    // Excel Export Service
    builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

    // Presenters
    builder.Services.AddScoped<IPresenter<InventarioEntity, InventarioViewModel>, InventarioPresenter>();

    // Job Services
    builder.Services.AddScoped<ParceDeliveryReport>();
    builder.Services.AddScoped<ParseTankInventoryReport>();
    builder.Services.AddScoped<DescargasJobs>();
    builder.Services.AddScoped<InventarioJob>();

    // Inventory Update Service
    builder.Services.AddSingleton<Web.Services.IInventoryUpdateService, Web.Services.InventoryUpdateService>();
}

// Add Identity services (same for both demo and production)
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

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = !isDemoMode)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    if (isDemoMode)
    {
        // Relaxed settings for demo
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 3;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
        Console.WriteLine("? Configuración de Identity relajada para demo");
    }
    else
    {
        // Production settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;
        options.SignIn.RequireConfirmedEmail = true;
        options.SignIn.RequireConfirmedAccount = true;
    }

    // Common settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
});

// Configure application cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = isDemoMode ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
});

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddDateRangePicker(config =>
{
    config.Attributes = new Dictionary<string, object>
    {
        {"class","form-control form-control-sm" }
    };
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Get the configured URLs
var urls = builder.Configuration["urls"] ?? builder.Configuration.GetSection("applicationUrl").Value;
if (!string.IsNullOrEmpty(urls))
{
    Console.WriteLine($"Web (Blazor) URLs configuradas: {urls}");
}

// Listen for when the app actually starts to show real URLs
app.Lifetime.ApplicationStarted.Register(() =>
{
    var addressFeature = app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
        .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
    
    if (addressFeature?.Addresses.Any() == true)
    {
        Console.WriteLine($"Web (Blazor) ejecutándose en las URLs:");
        foreach (var address in addressFeature.Addresses)
        {
            Console.WriteLine($"  - {address}");
        }
        
        if (isDemoMode)
        {
            Console.WriteLine();
            Console.WriteLine("?? MODO DEMO ACTIVADO ??");
            Console.WriteLine("- Base de datos en memoria");
            Console.WriteLine("- Puerto serial simulado");
            Console.WriteLine("- Servicio SOAP simulado");
            Console.WriteLine("- Autenticación simplificada");
            Console.WriteLine();
        }
    }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment() && !app.Environment.EnvironmentName.Equals("Demo"))
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
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
