using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
using Infrastructure.Communication;
using Infrastructure.Models;
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
            Console.WriteLine("=== INICIO DE EJECUCIÓN InventarioJob ===");
            Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");

            try
            {
                // 1. Validación y lectura de configuración
                Console.WriteLine("1. Leyendo configuración...");
                var idEstacionStr = _config["Estacion:Id"];
                string command = "i20100";

                // Crear objeto de configuración del puerto serial desde appsettings
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

                Console.WriteLine($"   - Puerto configurado: {serialPortSettings.PortName}");
                Console.WriteLine($"   - BaudRate configurado: {serialPortSettings.BaudRate}");
                Console.WriteLine($"   - DataBits configurado: {serialPortSettings.DataBits}");
                Console.WriteLine($"   - Parity configurado: {serialPortSettings.Parity}");
                Console.WriteLine($"   - StopBits configurado: {serialPortSettings.StopBits}");
                Console.WriteLine($"   - Handshake configurado: {serialPortSettings.Handshake}");
                Console.WriteLine($"   - ReadTimeout configurado: {serialPortSettings.ReadTimeout}ms");
                Console.WriteLine($"   - WriteTimeout configurado: {serialPortSettings.WriteTimeout}ms");
                Console.WriteLine($"   - ID Estación configurado: {idEstacionStr ?? "NULL"}");
                Console.WriteLine($"   - Comando a enviar: {command}");

                // Validaciones de configuración
                if (string.IsNullOrWhiteSpace(serialPortSettings.PortName))
                {
                    Console.WriteLine("ERROR: Puerto serial no configurado en appsettings");
                    return;
                }

                if (serialPortSettings.BaudRate <= 0)
                {
                    Console.WriteLine($"ERROR: BaudRate inválido: {serialPortSettings.BaudRate}");
                    return;
                }

                if (!int.TryParse(idEstacionStr, out int idEstacion))
                {
                    Console.WriteLine($"ERROR: ID de estación inválido: {idEstacionStr}");
                    return;
                }

                Console.WriteLine("   ✓ Configuración validada correctamente");

                // 2. Verificación del servicio serial
                Console.WriteLine("2. Verificando servicio serial...");
                if (_serialPortService == null)
                {
                    Console.WriteLine("ERROR: _serialPortService es NULL");
                    return;
                }

                Console.WriteLine($"   - Tipo de servicio serial: {_serialPortService.GetType().Name}");

                // Verificar si es SerialPortManager para usar funcionalidad async
                if (_serialPortService is SerialPortManager serialPortManager)
                {
                    Console.WriteLine("   ✓ Usando SerialPortManager para comunicación async");

                    string response;
                    try
                    {
                        response = await serialPortManager.SendCommandAsync(serialPortSettings, command, timeoutMs: 10000);
                        
                        Console.WriteLine("   ✓ Comando enviado y respuesta recibida");
                        Console.WriteLine($"   - Longitud de respuesta: {response?.Length ?? 0} caracteres");
                        
                        if (!string.IsNullOrWhiteSpace(response))
                        {
                            // Mostrar los primeros y últimos caracteres para debug
                            var preview = response.Length > 100 
                                ? $"{response[..50]}...{response[^50..]}" 
                                : response;
                            Console.WriteLine($"   - Vista previa de respuesta: {preview}");
                        }
                    }
                    catch (TaskCanceledException tcEx)
                    {
                        Console.WriteLine($"ERROR: Timeout en comunicación serial después de 10 segundos");
                        Console.WriteLine($"   - Mensaje: {tcEx.Message}");
                        Console.WriteLine("   - Posible causa: Dispositivo no responde o puerto ocupado");
                        throw;
                    }
                    catch (InvalidOperationException ioEx)
                    {
                        Console.WriteLine($"ERROR: Operación inválida en puerto serial");
                        Console.WriteLine($"   - Mensaje: {ioEx.Message}");
                        Console.WriteLine("   - Posible causa: Puerto no disponible o ya en uso");
                        throw;
                    }
                    catch (UnauthorizedAccessException uaEx)
                    {
                        Console.WriteLine($"ERROR: Acceso no autorizado al puerto serial");
                        Console.WriteLine($"   - Mensaje: {uaEx.Message}");
                        Console.WriteLine("   - Posible causa: Puerto en uso por otra aplicación");
                        throw;
                    }
                    catch (Exception commEx)
                    {
                        Console.WriteLine($"ERROR: Excepción no controlada en comunicación serial");
                        Console.WriteLine($"   - Tipo: {commEx.GetType().Name}");
                        Console.WriteLine($"   - Mensaje: {commEx.Message}");
                        Console.WriteLine($"   - StackTrace: {commEx.StackTrace}");
                        throw;
                    }

                    // 4. Validación de respuesta
                    Console.WriteLine("4. Validando respuesta...");
                    if (string.IsNullOrWhiteSpace(response))
                    {
                        Console.WriteLine("ERROR: No se recibió respuesta del puerto serial.");
                        Console.WriteLine("   - La respuesta está vacía o es null");
                        return;
                    }

                    Console.WriteLine("   ✓ Respuesta recibida y no está vacía");

                    // 5. Creación de scope para servicios
                    Console.WriteLine("5. Creando scope para servicios...");
                    using var scope = _scopeFactory.CreateScope();
                    var inventarioService = scope.ServiceProvider.GetRequiredService<InventarioService<InventarioEntity, InventarioViewModel>>();
                    
                    if (inventarioService == null)
                    {
                        Console.WriteLine("ERROR: No se pudo obtener InventarioService del scope");
                        return;
                    }

                    Console.WriteLine("   ✓ Scope creado e InventarioService obtenido");

                    // 6. Parsing de la respuesta
                    Console.WriteLine("6. Procesando respuesta del dispositivo...");
                    TankReport result;
                    
                    try
                    {
                        result = _parseTankInventoryReport.Execute(response);
                        Console.WriteLine("   ✓ Respuesta parseada correctamente");
                        Console.WriteLine($"   - Fecha del reporte: {result.Date}");
                        Console.WriteLine($"   - Número de tanques encontrados: {result.Tanks?.Count ?? 0}");
                    }
                    catch (ArgumentException argEx)
                    {
                        Console.WriteLine($"ERROR: Error en el formato de los datos recibidos");
                        Console.WriteLine($"   - Mensaje: {argEx.Message}");
                        Console.WriteLine($"   - Respuesta recibida: {response}");
                        throw;
                    }
                    catch (Exception parseEx)
                    {
                        Console.WriteLine($"ERROR: Error al parsear la respuesta");
                        Console.WriteLine($"   - Tipo: {parseEx.GetType().Name}");
                        Console.WriteLine($"   - Mensaje: {parseEx.Message}");
                        Console.WriteLine($"   - Respuesta recibida: {response}");
                        throw;
                    }

                    // 7. Validación de tanques
                    Console.WriteLine("7. Validando datos de tanques...");
                    if (result.Tanks == null || !result.Tanks.Any())
                    {
                        Console.WriteLine("WARNING: No se encontraron datos de tanques en la respuesta.");
                        Console.WriteLine("   - Posible causa: Respuesta vacía del dispositivo o formato incorrecto");
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
                        catch (Exception dbEx)
                        {
                            errorCount++;
                            Console.WriteLine($"   ✗ Error al guardar inventario para tanque {tank.NoTank}:");
                            Console.WriteLine($"     - Tipo: {dbEx.GetType().Name}");
                            Console.WriteLine($"     - Mensaje: {dbEx.Message}");
                            
                            if (dbEx.InnerException != null)
                            {
                                Console.WriteLine($"     - Excepción interna: {dbEx.InnerException.Message}");
                            }
                        }
                    }

                    // 9. Resumen final
                    Console.WriteLine("9. Resumen de ejecución:");
                    Console.WriteLine($"   - Tanques procesados: {tankCount}");
                    Console.WriteLine($"   - Guardados exitosamente: {successCount}");
                    Console.WriteLine($"   - Errores: {errorCount}");
                    Console.WriteLine("=== FIN DE EJECUCIÓN InventarioJob ===");
                }
                else
                {
                    Console.WriteLine($"ERROR: El servicio serial no es compatible con operaciones async");
                    Console.WriteLine($"   - Tipo actual: {_serialPortService.GetType().Name}");
                    Console.WriteLine($"   - Se requiere SerialPortManager para jobs");
                    Console.WriteLine("   - Verifica la configuración de dependencias en ServiceCollectionExtensions");
                    return;
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("ERROR FINAL: La comunicación serial se ha cancelado por tiempo de espera agotado.");
                Console.WriteLine("   - Verifica que el dispositivo esté conectado y funcionando");
                Console.WriteLine("   - Verifica que el puerto COM esté disponible");
                throw;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"ERROR FINAL: Error en el formato de los datos recibidos: {ex.Message}");
                Console.WriteLine("   - La respuesta del dispositivo no tiene el formato esperado");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR FINAL: Error durante la comunicación serial: {ex.Message}");
                Console.WriteLine($"   - Tipo de excepción: {ex.GetType().Name}");
                Console.WriteLine($"   - StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   - Causa raíz: {ex.InnerException.Message}");
                    Console.WriteLine($"   - Tipo de causa raíz: {ex.InnerException.GetType().Name}");
                }
                
                Console.WriteLine("=== ERROR EN InventarioJob ===");
                throw;
            }
        }
    }
}
