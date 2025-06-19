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
        public ParceDeliveryReport _parceDeliveryReport;

        public DescargasJobs(
            IConfiguration config,
            ISerialPortService serialPortService,
            IServiceScopeFactory scopeFactory,
            ParceDeliveryReport parceDeliveryReport)
        {
            _config = config;
            _serialPortService = serialPortService;
            _scopeFactory = scopeFactory;
            _parceDeliveryReport = parceDeliveryReport;
        }

        public async Task Execute()
        {
            var portName = _config["SerialPort:PortName"];
            var baudRate = int.Parse(_config["SerialPort:BaudRate"]);
            var idEstacion = int.Parse(_config["Estacion:Id"]);
            string command = "i20200";

            try
            {
                // Comunicación serial robusta y serializada
                string response = await (_serialPortService as SerialPortManager)
                    .SendCommandAsync(portName, baudRate, command, timeoutMs: 40000);
                
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
                var descargasService = scope.ServiceProvider.GetRequiredService<DescargasService<DescargasEntity>>();

                DeliveryTankReport result = _parceDeliveryReport.Execute(response);

                // Validar que hay tanques en el resultado
                if (result.Tanks == null || !result.Tanks.Any())
                {
                    Console.WriteLine("No se encontraron datos de tanques en la respuesta.");
                    return;
                }

                foreach (var tank in result.Tanks)
                {
                    // Validar que existan entregas para el tanque
                    if (tank.Deliveries == null || !tank.Deliveries.Any())
                    {
                        Console.WriteLine($"No hay entregas registradas para el tanque {tank.NoTank}");
                        continue;
                    }

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
                        Console.WriteLine($"Registro guardado para tanque {tank.NoTank}");
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
