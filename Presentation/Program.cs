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

var provider = services.BuildServiceProvider();

// Ejecución
var serialService = provider.GetRequiredService<SerialService>();
await serialService.RunAsync();