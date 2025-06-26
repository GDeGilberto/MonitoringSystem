using API.Extensions;

var builder = WebApplication.CreateBuilder(args);

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

// Print current environment and URLs to console
Console.WriteLine($"API ejecutándose en el environment: {app.Environment.EnvironmentName}");

// Get the configured URLs
var urls = builder.Configuration["urls"] ?? builder.Configuration.GetSection("applicationUrl").Value;
if (!string.IsNullOrEmpty(urls))
{
    Console.WriteLine($"API URLs configuradas: {urls}");
}

// Listen for when the app actually starts to show real URLs
app.Lifetime.ApplicationStarted.Register(() =>
{
    var addressFeature = app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
        .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
    
    if (addressFeature?.Addresses.Any() == true)
    {
        Console.WriteLine($"API ejecutándose en las URLs:");
        foreach (var address in addressFeature.Addresses)
        {
            Console.WriteLine($"  - {address}");
        }
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

app.Run();
