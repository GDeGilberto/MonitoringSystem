using Application.Services;
using Domain.Interfaces;
using Infrastructure.Communication;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
services.AddSingleton<ISerialCommunication>(_ =>
    new SerialCommunication(
        portName: configuration["SerialPort:PortName"] ?? "COM1",
        baudRate: int.Parse(configuration["SerialPort:BaudRate"] ?? "9600")));

services.AddScoped<SerialService>();

var provider = services.BuildServiceProvider();

// Ejecución
try
{
    var serialService = provider.GetRequiredService<SerialService>();
    serialService.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Error crítico: {ex.Message}");
    Thread.Sleep(5000);
}