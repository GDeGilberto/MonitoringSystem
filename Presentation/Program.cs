using Application.UseCases;
using Domain.Interfaces;
using Infrastructure.Communication;
using Infrastructure.Data;
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

// Configuración del servicio serial
services.AddSingleton<ISerialPortService>(provider =>
    new SerialPortService());
services.AddSingleton<ISerialPortService, SerialPortService>();

services.AddScoped<ParceDeliveryReport>();
services.AddScoped<ParseTankInventoryReport>();
services.AddScoped<TerminalConsole>();

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