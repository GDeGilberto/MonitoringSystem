using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
using Infrastructure.Communication;
using Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs
{
    public class DescargasJobs
    {
        private readonly IConfiguration _config;
        private readonly ISerialPortService _serialPortService;
        private readonly ParceDeliveryReport _parceDeliveryReport;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DescargasJobs> _logger;

        public DescargasJobs(
            IConfiguration config,
            ISerialPortService serialPortService,
            ParceDeliveryReport parceDeliveryReport,
            IServiceScopeFactory scopeFactory,
            ILogger<DescargasJobs> logger)
        {
            _config = config;
            _serialPortService = serialPortService;
            _parceDeliveryReport = parceDeliveryReport;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task Execute()
        {
            _logger.LogInformation("=== INICIO DE EJECUCIÓN DescargasJob ===");

            try
            {
                var idEstacionStr = _config["Estacion:Id"];
                const string command = "i20200";

                var serialPortSettings = new SerialPortSettings
                {
                    PortName = _config["SerialPort:PortName"] ?? "COM1",
                    BaudRate = int.TryParse(_config["SerialPort:BaudRate"], out int baudRate) ? baudRate : 9600,
                    DataBits = int.TryParse(_config["SerialPort:DataBits"], out int dataBits) ? dataBits : 8,
                    Parity = _config["SerialPort:Parity"] ?? "None",
                    StopBits = _config["SerialPort:StopBits"] ?? "One",
                    Handshake = _config["SerialPort:Handshake"] ?? "None",
                    ReadTimeout = int.TryParse(_config["SerialPort:ReadTimeout"], out int readTimeout) ? readTimeout : 500,
                    WriteTimeout = int.TryParse(_config["SerialPort:WriteTimeout"], out int writeTimeout) ? writeTimeout : 500
                };

                _logger.LogInformation("Configuración - Puerto: {Port}, BaudRate: {BaudRate}, Estación: {Estacion}", 
                    serialPortSettings.PortName, serialPortSettings.BaudRate, idEstacionStr);

                if (string.IsNullOrWhiteSpace(serialPortSettings.PortName))
                {
                    _logger.LogError("Puerto serial no configurado en appsettings");
                    return;
                }

                if (serialPortSettings.BaudRate <= 0)
                {
                    _logger.LogError("BaudRate inválido: {BaudRate}", serialPortSettings.BaudRate);
                    return;
                }

                if (!int.TryParse(idEstacionStr, out int idEstacion))
                {
                    _logger.LogError("ID de estación inválido: {IdEstacion}", idEstacionStr);
                    return;
                }

                if (_serialPortService == null)
                {
                    _logger.LogError("SerialPortService es NULL");
                    return;
                }

                Console.WriteLine($"   - Tipo de servicio serial: {_serialPortService.GetType().Name}");

                // Verificar si es SerialPortManager para usar funcionalidad async
                if (_serialPortService is SerialPortManager serialPortManager)
                {
                    _logger.LogInformation("Usando SerialPortManager para comunicación async");

                    string response;
                    try
                    {
                        response = await serialPortManager.SendCommandAsync(serialPortSettings, command, timeoutMs: 40000);
                        _logger.LogInformation("Comando enviado correctamente. Respuesta: {Length} caracteres", response?.Length ?? 0);
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogError("Timeout en comunicación serial después de 40 segundos");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error en comunicación serial");
                        throw;
                    }

                    if (string.IsNullOrWhiteSpace(response))
                    {
                        _logger.LogWarning("No se recibió respuesta del puerto serial");
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var descargasService = scope.ServiceProvider.GetRequiredService<DescargasService<DescargasEntity>>();

                    if (descargasService == null)
                    {
                        _logger.LogError("No se pudo obtener DescargasService del scope");
                        return;
                    }

                    DeliveryTankReport result;
                    try
                    {
                        result = _parceDeliveryReport.Execute(response);
                        _logger.LogInformation("Respuesta parseada correctamente. Fecha: {Date}, Tanques: {Count}", 
                            result.Date, result.Tanks?.Count ?? 0);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al parsear la respuesta de descargas");
                        throw;
                    }

                    if (result.Tanks == null || !result.Tanks.Any())
                    {
                        _logger.LogWarning("No se encontraron datos de tanques en la respuesta");
                        return;
                    }

                    Console.WriteLine($"   ✓ Se encontraron {result.Tanks.Count} tanques para procesar");

                    // 8. Procesamiento de tanques
                    Console.WriteLine("8. Guardando datos de descargas...");
                    int tankCount = 0;
                    int successCount = 0;
                    int errorCount = 0;
                    int skippedCount = 0;

                    foreach (var tank in result.Tanks)
                    {
                        tankCount++;
                        Console.WriteLine($"   Procesando tanque {tankCount}/{result.Tanks.Count}:");
                        Console.WriteLine($"   - No. Tanque: {tank.NoTank}");

                        // Validar que existan entregas para el tanque
                        if (tank.Deliveries == null || !tank.Deliveries.Any())
                        {
                            skippedCount++;
                            _logger.LogInformation("Tanque {NoTanque} omitido (sin entregas)", tank.NoTank);
                            continue;
                        }

                        try
                        {
                            var firstDelivery = tank.Deliveries.First();
                            var volumenInicial = firstDelivery.Start.Volume;
                            var volumenDisponible = firstDelivery.End.Volume;

                            DescargasEntity descarga = new(
                                idEstacion,
                                tank.NoTank,
                                volumenInicial,
                                firstDelivery.Start.Temperature,
                                firstDelivery.Start.Date,
                                volumenDisponible,
                                firstDelivery.End.Temperature,
                                firstDelivery.End.Date,
                                volumenDisponible - volumenInicial
                            );

                            await descargasService.AddAsync(descarga);
                            successCount++;
                            Console.WriteLine($"   ✓ Registro guardado para tanque {tank.NoTank}");
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            _logger.LogError(ex, "Error al guardar descarga para tanque {NoTanque}", tank.NoTank);
                            }
                        }

                    _logger.LogInformation("Resumen: {TotalTanques} tanques procesados, {Exitosos} exitosos, {Omitidos} omitidos, {Errores} errores", 
                        result.Tanks.Count, successCount, skippedCount, errorCount);
                }
                else
                {
                    _logger.LogError("El servicio serial no es compatible con operaciones async. Tipo: {Type}", 
                        _serialPortService.GetType().Name);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la ejecución del DescargasJob");
                throw;
            }
            finally
            {
                _logger.LogInformation("=== FIN DE EJECUCIÓN DescargasJob ===");
            }
        }
    }
}
