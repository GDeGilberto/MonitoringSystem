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

// Add HTTP client services
builder.Services.AddHttpClientServices();

// Add repositories, mappers, services, use cases, and jobs
builder.Services
    .AddRepositories()
    .AddMappers()
    .AddServices()
    .AddUseCases()
    .AddJobServices();

var app = builder.Build();

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
