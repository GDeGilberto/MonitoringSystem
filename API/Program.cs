using API.Extensions;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Add core services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Add authentication and authorization
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // Add Swagger documentation
    builder.Services.AddSwaggerDocumentation();

    // Add database context
    builder.Services.AddDatabaseContext(builder.Configuration);

    // Add Hangfire services
    builder.Services.AddHangfireServices(builder.Configuration);

    // Add serial port services
    builder.Services.AddSerialPortServices();

    // Add repositories, mappers, services, use cases, and jobs
    builder.Services
        .AddRepositories()
        .AddMappers()
        .AddServices()
        .AddUseCases()
        .AddJobServices();

    var app = builder.Build();

    Console.WriteLine($"API ejecut�ndose en el environment: {app.Environment.EnvironmentName}");

    // Get the configured URLs
    var urls = builder.Configuration["urls"] ?? builder.Configuration.GetSection("applicationUrl").Value;
    if (!string.IsNullOrEmpty(urls))
    {
        Console.WriteLine($"API URLs configuradas: {urls}");
    }

    // Listen for when the app actually starts to show real URLs
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            var addressFeature = app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
                .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();

            if (addressFeature?.Addresses.Any() == true)
            {
                Console.WriteLine($"API ejecut�ndose en las URLs:");
                foreach (var address in addressFeature.Addresses)
                {
                    Console.WriteLine($"  - {address}");
                }
                Console.WriteLine("API iniciada correctamente y lista para recibir solicitudes.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener direcciones del servidor: {ex.Message}");
        }
    });

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerDocumentation();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    // Configure Hangfire dashboard and jobs
    app.UseHangfireDashboardWithSecurity();
    app.ConfigureRecurringJobs();

    app.MapControllers();

    Console.WriteLine("Iniciando servidor...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Error cr�tico durante el inicio de la aplicaci�n: {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");

    if (ex.InnerException != null)
    {
        Console.WriteLine($"Excepci�n interna: {ex.InnerException.Message}");
        Console.WriteLine($"StackTrace interno");
    }
}