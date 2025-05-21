using Microsoft.Extensions.DependencyInjection;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Communication;

var services = new ServiceCollection();

// Configuración de DI
services.AddTransient<ISerialCommunication>(_ =>
    new SerialCommunication("COM3", 2400));
services.AddTransient<SerialService>();

var provider = services.BuildServiceProvider();

// Ejecución
var serialService = provider.GetRequiredService<SerialService>();
await serialService.RunAsync();