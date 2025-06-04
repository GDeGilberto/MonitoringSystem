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


        public InventarioJob(IConfiguration config,
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

            // Comunicación serial robusta y serializada
            string response = await (_serialPortService as SerialPortManager)
                .SendCommandAsync(portName, baudRate, command, timeoutMs: 10000);

            using var scope = _scopeFactory.CreateScope();
            var inventarioService = scope.ServiceProvider.GetRequiredService<InventarioService<ProcInventarioEntity, InventarioViewModel>>();

            TankReport result = _parseTankInventoryReport.Execute(response);

            foreach (var tank in result.Tanks)
            {
                ProcInventarioEntity inventario = new()
                {
                    IdEstacion = idEstacion,
                    NoTanque = tank.NoTank,
                    ClaveProducto = "",
                    VolumenDisponible = tank.TankData.Volume,
                    Temperatura = tank.TankData.Temperature,
                    Fecha = DateTime.Now
                };

                try
                {
                    await inventarioService.AddAsync(inventario);
                    Console.WriteLine("Guardado exitoso");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear inventario para tanque {tank.NoTank}: {ex.Message}");
                }
            }
        }

    }
}
