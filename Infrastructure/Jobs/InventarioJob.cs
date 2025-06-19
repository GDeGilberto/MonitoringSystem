using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
using Infrastructure.Communication;
using Infrastructure.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Jobs
{
    public class InventarioJob
    {
        private readonly IConfiguration _config;
        private ISerialPortService _serialPortService;
        private ParseTankInventoryReport _parseTankInventoryReport;
        private readonly IServiceScopeFactory _scopeFactory;

        public InventarioJob(
            IConfiguration config,
            ISerialPortService serialPortService,
            ParseTankInventoryReport parseTankInventoryReport,
            IServiceScopeFactory scopeFactory)
        {
            _config = config;
            _serialPortService = serialPortService;
            _parseTankInventoryReport = parseTankInventoryReport;
            _scopeFactory = scopeFactory;
        }

        public async Task Execute()
        {
            var portName = _config["SerialPort:PortName"];
            var baudRate = int.Parse(_config["SerialPort:BaudRate"]);
            var idEstacion = int.Parse(_config["Estacion:Id"]);
            string command = "i20100";

            try
            {
                // Comunicación serial robusta y serializada
                string response = await (_serialPortService as SerialPortManager)
                    .SendCommandAsync(portName, baudRate, command, timeoutMs: 10000);
                
                // Verificar si la respuesta está vacía o es inválida
                if (string.IsNullOrWhiteSpace(response))
                {
                    Console.WriteLine("Error: No se recibió respuesta del puerto serial.");
                    return;
                }

                // Validar si la respuesta indica "Sin resultado" (código FC)
                if (response.Contains("FC"))
                {
                    Console.WriteLine("Sin ningún resultado en la respuesta.");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var inventarioService = scope.ServiceProvider.GetRequiredService<InventarioService<InventarioEntity, InventarioViewModel>>();

                TankReport result = _parseTankInventoryReport.Execute(response);

                // Validar que hay tanques en el resultado
                if (result.Tanks == null || !result.Tanks.Any())
                {
                    Console.WriteLine("No se encontraron datos de tanques en la respuesta.");
                    return;
                }

                foreach (var tank in result.Tanks)
                {
                    InventarioEntity inventario = new(
                        idEstacion,
                        tank.NoTank,
                        "",
                        tank.TankData.Volume,
                        tank.TankData.Temperature,
                        DateTime.Now
                    );

                    try
                    {
                        await inventarioService.AddAsync(inventario);
                        Console.WriteLine($"Inventario guardado exitosamente para tanque {tank.NoTank}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al crear inventario para tanque {tank.NoTank}: {ex.Message}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Error: La comunicación serial se ha cancelado por tiempo de espera agotado.");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error en el formato de los datos recibidos: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la comunicación serial: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Causa: {ex.InnerException.Message}");
                }
            }
        }
    }
}
