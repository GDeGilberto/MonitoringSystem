using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
using Infrastructure.Communication;
using Infrastructure.Models;
using Infrastructure.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs
{
    public class InventarioJob
    {
        private readonly IConfiguration _config;
        private readonly ISerialPortService _serialPortService;
        private readonly ParseTankInventoryReport _parseTankInventoryReport;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InventarioJob> _logger;

        public InventarioJob(
            IConfiguration config,
            ISerialPortService serialPortService,
            ParseTankInventoryReport parseTankInventoryReport,
            IServiceScopeFactory scopeFactory,
            ILogger<InventarioJob> logger)
        {
            _config = config;
            _serialPortService = serialPortService;
            _parseTankInventoryReport = parseTankInventoryReport;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task Execute()
        {
            _logger.LogInformation("=== INICIO DE EJECUCIÓN InventarioJob ===");

            try
            {
                var idEstacionStr = _config["Estacion:Id"];
                const string command = "i20100";

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

                Console.WriteLine("   ✓ Configuración validada correctamente");

                // 2. Verificación del servicio serial
                Console.WriteLine("2. Verificando servicio serial...");
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
                        response = await serialPortManager.SendCommandAsync(serialPortSettings, command, timeoutMs: 10000);
                        _logger.LogInformation("Comando enviado correctamente. Respuesta: {Length} caracteres", response?.Length ?? 0);
                        }
                    catch (TaskCanceledException)
                    {
                        _logger.LogError("Timeout en comunicación serial después de 10 segundos");
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
                    var inventarioService = scope.ServiceProvider.GetRequiredService<InventarioService<InventarioEntity, InventarioViewModel>>();
                    
                    if (inventarioService == null)
                    {
                        _logger.LogError("No se pudo obtener InventarioService del scope");
                        return;
                    }

                    TankReport result;
                    try
                    {
                        result = _parseTankInventoryReport.Execute(response);
                        _logger.LogInformation("Respuesta parseada correctamente. Fecha: {Date}, Tanques: {Count}", 
                            result.Date, result.Tanks?.Count ?? 0);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al parsear la respuesta");
                        throw;
                    }

                    if (result.Tanks == null || !result.Tanks.Any())
                    {
                        _logger.LogWarning("No se encontraron datos de tanques en la respuesta");
                        return;
                    }

                    Console.WriteLine($"   ✓ Se encontraron {result.Tanks.Count} tanques para procesar");

                    // 8. Procesamiento de tanques
                    Console.WriteLine("8. Guardando datos de inventario...");
                    int tankCount = 0;
                    int successCount = 0;
                    int errorCount = 0;

                    foreach (var tank in result.Tanks)
                    {
                        tankCount++;
                        Console.WriteLine($"   Procesando tanque {tankCount}/{result.Tanks.Count}:");
                        Console.WriteLine($"   - No. Tanque: {tank.NoTank}");
                        Console.WriteLine($"   - Volumen: {tank.TankData.Volume}");
                        Console.WriteLine($"   - Temperatura: {tank.TankData.Temperature}");

                        try
                        {
                            InventarioEntity inventario = new(
                                idEstacion,
                                tank.NoTank,
                                "",
                                tank.TankData.Volume,
                                tank.TankData.Temperature,
                                DateTime.Now
                            );

                            await inventarioService.AddAsync(inventario);
                            successCount++;
                            Console.WriteLine($"   ✓ Inventario guardado exitosamente para tanque {tank.NoTank}");
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            _logger.LogError(ex, "Error al guardar inventario para tanque {NoTanque}", tank.NoTank);
                            }
                        }

                    _logger.LogInformation("Resumen: {TotalTanques} tanques procesados, {Exitosos} exitosos, {Errores} errores", 
                        result.Tanks.Count, successCount, errorCount);
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
                _logger.LogError(ex, "Error durante la ejecución del InventarioJob");
                throw;
            }
            finally
            {
                _logger.LogInformation("=== FIN DE EJECUCIÓN InventarioJob ===");
            }
        }
    }
}
