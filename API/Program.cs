using API.Extensions;
using API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add controllers and API explorer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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
    .AddJobs();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

// Add global exception handling middleware
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();
app.UseAuthorization();

// Configure Hangfire dashboard and jobs
app.UseHangfireDashboardWithSecurity();
app.ConfigureRecurringJobs();

app.MapControllers();

app.Run();
