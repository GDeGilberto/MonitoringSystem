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

        public DescargasJobs(IConfiguration config,
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

            // Comunicación serial robusta y serializada
            string response = await (_serialPortService as SerialPortManager)
                .SendCommandAsync(portName, baudRate, command, timeoutMs: 40000);

            using var scope = _scopeFactory.CreateScope();
            var descargasService = scope.ServiceProvider.GetRequiredService<DescargasService<ProcDescargasEntity>>();

            DeliveryTankReport result = _parceDeliveryReport.Execute(response);

            foreach (var tank in result.Tanks)
            {
                var volumenInicial = tank.Deliveries.FirstOrDefault()?.Start.Volume ?? 0;
                var volumenDisponible = tank.Deliveries.FirstOrDefault()?.End.Volume ?? 0;

                ProcDescargasEntity descarga = new()
                {
                    IdEstacion = idEstacion,
                    NoTanque = tank.NoTank,
                    VolumenInicial = volumenInicial,
                    TemperaturaInicial = tank.Deliveries.FirstOrDefault()?.Start.Temperature ?? 0,
                    FechaInicial = tank.Deliveries.FirstOrDefault()?.Start.Date ?? DateTime.MinValue,
                    VolumenDisponible = volumenDisponible,
                    TemperaturaFinal = tank.Deliveries.FirstOrDefault()?.End.Temperature ?? 0,
                    FechaFinal = tank.Deliveries.FirstOrDefault()?.End.Date ?? DateTime.MinValue,
                    CantidadCargada = volumenDisponible - volumenInicial
                };

                try
                {
                    await descargasService.AddAsync(descarga);
                    Console.WriteLine("Ultimos registros guardados");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear inventario para tanque {tank.NoTank}: {ex.Message}");
                }
            }
        }
    }
}
