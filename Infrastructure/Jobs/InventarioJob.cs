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
        private readonly IApiClientService _apiClient;

        public InventarioJob(
            IConfiguration config,
            ISerialPortService serialPortService,
            ParseTankInventoryReport parseTankInventoryReport,
            IServiceScopeFactory scopeFactory,
            IApiClientService apiClient)
        {
            _config = config;
            _serialPortService = serialPortService;
            _parseTankInventoryReport = parseTankInventoryReport;
            _scopeFactory = scopeFactory;
            _apiClient = apiClient;
        }

        public async Task Execute()
        {
            var portName = _config["SerialPort:PortName"];
            var baudRate = int.Parse(_config["SerialPort:BaudRate"]);
            var idEstacion = int.Parse(_config["Estacion:Id"]);
            string command = "i20100";

            // Comunicación serial robusta y serializada
            string response = await (_serialPortService as SerialPortManager)
                .SendCommandAsync(portName, baudRate, command, timeoutMs: 10000);

            using var scope = _scopeFactory.CreateScope();
            var inventarioService = scope.ServiceProvider.GetRequiredService<InventarioService<InventarioEntity, InventarioViewModel>>();

            TankReport result = _parseTankInventoryReport.Execute(response);

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
                    Console.WriteLine("Guardado exitoso");
                    
                    // Example of calling a protected API endpoint using the API key authentication
                    try
                    {
                        // You can call your protected endpoints using the API client
                        var apiResponse = await _apiClient.GetAsync<object>("api/JobApi/apikey-protected");
                        Console.WriteLine("API protected endpoint accessed successfully");
                    }
                    catch (Exception apiEx)
                    {
                        Console.WriteLine($"Error accessing API endpoint: {apiEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear inventario para tanque {tank.NoTank}: {ex.Message}");
                }
            }
        }
    }
}
