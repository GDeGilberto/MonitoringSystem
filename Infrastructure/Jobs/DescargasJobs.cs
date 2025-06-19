using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
using Infrastructure.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Jobs
{
    public class DescargasJobs
    {
        private readonly IConfiguration _config;
        private readonly ISerialPortService _serialPortService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IApiClientService _apiClient;
        public ParceDeliveryReport _parceDeliveryReport;

        public DescargasJobs(
            IConfiguration config,
            ISerialPortService serialPortService,
            IServiceScopeFactory scopeFactory,
            ParceDeliveryReport parceDeliveryReport,
            IApiClientService apiClient)
        {
            _config = config;
            _serialPortService = serialPortService;
            _scopeFactory = scopeFactory;
            _parceDeliveryReport = parceDeliveryReport;
            _apiClient = apiClient;
        }

        public async Task Execute()
        {
            var portName = _config["SerialPort:PortName"];
            var baudRate = int.Parse(_config["SerialPort:BaudRate"]);
            var idEstacion = int.Parse(_config["Estacion:Id"]);
            string command = "i20200";

            // Comunicación serial robusta y serializada
            string response = await (_serialPortService as SerialPortManager)
                .SendCommandAsync(portName, baudRate, command, timeoutMs: 40000);

            using var scope = _scopeFactory.CreateScope();
            var descargasService = scope.ServiceProvider.GetRequiredService<DescargasService<DescargasEntity>>();

            DeliveryTankReport result = _parceDeliveryReport.Execute(response);

            foreach (var tank in result.Tanks)
            {
                var volumenInicial = tank.Deliveries.FirstOrDefault()?.Start.Volume ?? 0;
                var volumenDisponible = tank.Deliveries.FirstOrDefault()?.End.Volume ?? 0;

                DescargasEntity descarga = new(
                    idEstacion,
                    tank.NoTank,
                    volumenInicial,
                    tank.Deliveries.FirstOrDefault()?.Start.Temperature ?? 0,
                    tank.Deliveries.FirstOrDefault()?.Start.Date ?? DateTime.MinValue,
                    volumenDisponible,
                    tank.Deliveries.FirstOrDefault()?.End.Temperature ?? 0,
                    tank.Deliveries.FirstOrDefault()?.End.Date ?? DateTime.MinValue,
                    volumenDisponible - volumenInicial
                );

                try
                {
                    await descargasService.AddAsync(descarga);
                    Console.WriteLine("Ultimos registros guardados");
                    
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
