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

    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("API ejecutándose en el environment: {Environment}", app.Environment.EnvironmentName);

    // Listen for when the app actually starts to show real URLs
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            var addressFeature = app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
                .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();

            if (addressFeature?.Addresses.Any() == true)
            {
                logger.LogInformation("API ejecutándose en las URLs: {Addresses}", string.Join(", ", addressFeature.Addresses));
                }
            }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error al obtener direcciones del servidor");
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

    logger.LogInformation("Iniciando servidor...");
    app.Run();
}
catch (Exception ex)
{
    var logger = new LoggerFactory().CreateLogger<Program>();
    logger.LogCritical(ex, "Error crítico durante el inicio de la aplicación");
    throw;
}