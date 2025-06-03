using Application.Interfaces;
using Application.Models;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Jobs
{
    public class DescargasJobs
    {
        private readonly IConfiguration _config;
        private readonly ISerialPortService _serialPortService;
        private readonly IServiceScopeFactory _scopeFactory;
        public DescargasService<ProcDescargasEntity> _descargasService;
        public ParceDeliveryReport _parceDeliveryReport;
        private EventHandler<string>? _currentResponseHandler;

        public DescargasJobs(IConfiguration config,
            ISerialPortService serialPortService,
            IServiceScopeFactory scopeFactory,
            DescargasService<ProcDescargasEntity> descargasService,
            ParceDeliveryReport parceDeliveryReport)
        {
            _config = config;
            _serialPortService = serialPortService;
            _scopeFactory = scopeFactory;
            _descargasService = descargasService;
            _parceDeliveryReport = parceDeliveryReport;
        }

        public void Execute()
        {
            var _portName = _config["SerialPort:PortName"];
            var _baudRate = int.Parse(_config["SerialPort:BaudRate"]);
            var _idEstacion = int.Parse(_config["Estacion:Id"]);
            string command = "i20200";

            if (!_serialPortService.IsOpen) // Debes agregar esta propiedad a tu servicio
            {
                _serialPortService.Initialize(_portName, _baudRate);
            }

            _serialPortService.Write(command);

            _serialPortService.ClearBuffer();

            if (_currentResponseHandler != null)
            {
                _serialPortService.CompleteResponseReceived -= _currentResponseHandler;
            }

            _currentResponseHandler = async (sender, response) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var descargasService = scope.ServiceProvider.GetRequiredService<DescargasService<ProcDescargasEntity>>();

                try
                {
                    DeliveryTankReport result = _parceDeliveryReport.Execute(response);

                    foreach (var tank in result.Tanks)
                    {
                        var volumenInicial = tank.Deliveries.FirstOrDefault()?.Start.Volume ?? 0;
                        var volumenDisponible = tank.Deliveries.FirstOrDefault()?.End.Volume ?? 0;

                        ProcDescargasEntity descarga = new()
                        {
                            IdEstacion = _idEstacion,
                            NoTanque = tank.NoTank,
                            VolumenInicial = volumenInicial,
                            TemperaturaInicial = tank.Deliveries.FirstOrDefault()?.Start.Temperature ?? 0,
                            FechaInicial = tank.Deliveries.FirstOrDefault().Start.Date,
                            VolumenDisponible = volumenDisponible,
                            TemperaturaFinal = tank.Deliveries.FirstOrDefault()?.End.Temperature ?? 0,
                            FechaFinal = tank.Deliveries.FirstOrDefault().End.Date,
                            CantidadCargada = volumenDisponible - volumenInicial
                        };

                        await descargasService.AddAsync(descarga);
                        Console.WriteLine("Ultimos registros guardados");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear inventario: {ex.Message}");
                }
                finally
                {
                    _serialPortService.CompleteResponseReceived -= _currentResponseHandler;
                    _currentResponseHandler = null;
                }
            };
            _serialPortService.CompleteResponseReceived += _currentResponseHandler;
        }
    }
}
