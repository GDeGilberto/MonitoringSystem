using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
using Infrastructure.Communication;
using Infrastructure.Data;
using Infrastructure.Presenters;
using Infrastructure.Repositories;
using Infrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Presentation;

try
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();

    var services = new ServiceCollection();

    // Configuración de logging
    services.AddLogging(builder =>
    {
        builder.SetMinimumLevel(LogLevel.Information);
    });

    // Configuración de la base de datos
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    
    services.AddSingleton<IConfiguration>(configuration);

    // Servicios
    services.AddSingleton<ISerialPortService, SerialPortService>();
    services.AddScoped<IRepository<DescargasEntity>, DescargasRepository>();
    services.AddScoped<IRepository<InventarioEntity>, InventarioRepository>();
    services.AddScoped<IPresenter<InventarioEntity, InventarioViewModel>, InventarioPresenter>();
    services.AddScoped<DescargasService<DescargasEntity>>();
    services.AddScoped<InventarioService<InventarioEntity, InventarioViewModel>>();
    services.AddScoped<ParceDeliveryReport>();
    services.AddScoped<ParseTankInventoryReport>();
    services.AddScoped<TerminalConsole>();
    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

    var provider = services.BuildServiceProvider();
    var logger = provider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Iniciando aplicación de terminal");
    
    var terminal = provider.GetRequiredService<TerminalConsole>();
    terminal.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error crítico: {ex.Message}");
    Console.ResetColor();
    Console.WriteLine("Presione cualquier tecla para salir...");
    Console.ReadKey();
}