using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
using Infrastructure.Communication;
using Infrastructure.Communication.Demo;
using Infrastructure.Data;
using Infrastructure.Jobs;
using Infrastructure.Models;
using Infrastructure.Presenters;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Demo;
using Infrastructure.Services;
using Infrastructure.Services.Demo;
using Infrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Web.Extensions
{
    public static class DemoServiceExtensions
    {
        public static IServiceCollection AddDemoServices(this IServiceCollection services, IConfiguration configuration)
        {
            var isDemoMode = configuration.GetValue<bool>("Demo:Enabled", false);
            
            if (isDemoMode)
            {
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                // Use In-Memory Database for demo
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("DemoDatabase");
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Register demo repositories
                services.AddScoped<IRepositorySearch<ProcDescargaModel, DescargasEntity>, DemoDescargasRepository>();
                services.AddScoped<IRepository<EstacionesEntity>, DemoEstacionesRepository>();
                services.AddScoped<IRepository<InventarioEntity>, DemoInventarioRepository>();
                services.AddScoped<IRepository<DescargasEntity>, DemoDescargasRepository>();
                services.AddScoped<IRepository<TanqueEntity>, DemoTanqueRepository>();
                services.AddScoped<ITanqueRepository, DemoTanqueRepository>();
                services.AddScoped<IRepositorySearch<ProcInventarioModel, InventarioEntity>, DemoInventarioRepository>();

                // Register demo serial port service
                services.AddSingleton<ISerialPortService, DemoSerialPortService>();

                // Register demo SOAP service
                services.AddScoped<IDagalSoapService, DemoDagalSoapService>();

                // Use cases remain the same
                services.AddScoped<GetDescargaSearchUseCase<ProcDescargaModel>>();
                services.AddScoped<GetLatestInventarioByStationUseCase<ProcInventarioModel>>();
                services.AddScoped<GetEstacionesByIdUseCase>();
                services.AddScoped<GetTanqueByEstacionAndNumeroUseCase>();

                // Application services
                services.AddScoped<DescargasService<DescargasEntity>>();
                services.AddScoped<InventarioService<InventarioEntity, InventarioViewModel>>();

                // Excel Export Service
                services.AddScoped<IExcelExportService, ExcelExportService>();

                // Presenters
                services.AddScoped<IPresenter<InventarioEntity, InventarioViewModel>, InventarioPresenter>();

                // Job Services (with demo behavior)
                services.AddScoped<ParceDeliveryReport>();
                services.AddScoped<ParseTankInventoryReport>();
                services.AddScoped<DescargasJobs>();
                services.AddScoped<InventarioJob>();

                // Inventory Update Service
                services.AddSingleton<Web.Services.IInventoryUpdateService, Web.Services.InventoryUpdateService>();

                // Add hosted service to populate demo data
                services.AddHostedService<DemoDataSeeder>();

                return services;
            }

            return services;
        }
    }

    public class DemoDataSeeder : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DemoDataSeeder> _logger;

        public DemoDataSeeder(IServiceProvider serviceProvider, ILogger<DemoDataSeeder> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                await context.Database.EnsureCreatedAsync(cancellationToken);
                _logger.LogInformation("Demo database initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing demo database");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}