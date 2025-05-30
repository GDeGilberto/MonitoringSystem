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
using Presentation;

// Configuración
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

// DI
var services = new ServiceCollection();

// Configuración de la base de datos
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
services.AddSingleton<IConfiguration>(configuration);

// Configuración del servicio serial
services.AddSingleton<ISerialPortService>(provider =>
    new SerialPortService());
services.AddSingleton<ISerialPortService, SerialPortService>();

services.AddScoped<IRepository<ProcDescargasEntity>, DescargasRepository>();
services.AddScoped<IRepository<ProcInventarioEntity>, InventarioRepository>();
services.AddScoped<IPresenter<ProcInventarioEntity, InventarioViewModel>, InventarioPresenter>();
services.AddScoped<DescargasService<ProcDescargasEntity>>();
services.AddScoped<InventarioService<ProcInventarioEntity, InventarioViewModel>>();

services.AddScoped<ParceDeliveryReport>();
services.AddScoped<ParseTankInventoryReport>();

services.AddScoped<TerminalConsole>();
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var provider = services.BuildServiceProvider();

// Ejecución
try
{
    var terminal = provider.GetRequiredService<TerminalConsole>();
    terminal.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Error crítico: {ex.Message}");
    Thread.Sleep(5000);
}