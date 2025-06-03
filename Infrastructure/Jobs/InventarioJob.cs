using Application.Interfaces;
using Application.Models;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
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
        private readonly InventarioService<ProcInventarioEntity, InventarioViewModel> _inventarioService;
        private readonly IServiceScopeFactory _scopeFactory;
        private EventHandler<string>? _currentResponseHandler;


        public InventarioJob(IConfiguration config,
            ISerialPortService serialPortService,
            ParseTankInventoryReport parseTankInventoryReport,
            InventarioService<ProcInventarioEntity, InventarioViewModel> service,
            IServiceScopeFactory scopeFactory)
        {
            _config = config;
            _serialPortService = serialPortService;
            _inventarioService = service;
            _parseTankInventoryReport = parseTankInventoryReport;
            _scopeFactory = scopeFactory;
        }


        public void Execute()
        {
            var _portName = _config["SerialPort:PortName"];
            var _baudRate = int.Parse(_config["SerialPort:BaudRate"]);
            var _idEstacion = int.Parse(_config["Estacion:Id"]);
            string command = "i20100";

            _serialPortService.Initialize(_portName, _baudRate);
            _serialPortService.Write(command);

            if (_currentResponseHandler != null)
            {
                _serialPortService.CompleteResponseReceived -= _currentResponseHandler;
            }

            _currentResponseHandler = async (sender, response) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var inventarioService = scope.ServiceProvider.GetRequiredService<InventarioService<ProcInventarioEntity, InventarioViewModel>>();

                TankReport result = _parseTankInventoryReport.Execute(response);

                foreach (var tank in result.Tanks)
                {
                    ProcInventarioEntity inventario = new()
                    {
                        IdEstacion = _idEstacion,
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

                // Remover el manejador después de usarlo
                _serialPortService.CompleteResponseReceived -= _currentResponseHandler;
                _currentResponseHandler = null;
            };
            _serialPortService.CompleteResponseReceived += _currentResponseHandler;
        }

    }
}
