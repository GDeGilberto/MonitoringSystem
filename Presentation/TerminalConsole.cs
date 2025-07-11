﻿using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
using Infrastructure.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO.Ports;

namespace Presentation
{
    public class TerminalConsole
    {
        private readonly InventarioService<InventarioEntity, InventarioViewModel> _inventarioService;
        private readonly DescargasService<DescargasEntity> _descargasService;
        private readonly ISerialPortService _serialPortService;
        private readonly object _consoleLock = new();
        private readonly Stopwatch _responseStopwatch = new();
        public  ParceDeliveryReport _parceDeliveryReport;
        public  ParseTankInventoryReport _parseTankInventoryReport;
        private EventHandler<string>? _currentResponseHandler;

        private string _portName;
        private int _baudRate;
        private readonly int _idEstacion;
        private bool _isReceivingData = false;

        public TerminalConsole(
            ISerialPortService serialPortService, 
            ParceDeliveryReport parceDeliveryReport,
            ParseTankInventoryReport parseTankInventoryReport,
            InventarioService<InventarioEntity, InventarioViewModel> inventarioService,
            DescargasService<DescargasEntity> descargasService,
            IConfiguration config)
        {
            _serialPortService = serialPortService;
            _parceDeliveryReport = parceDeliveryReport;
            _parseTankInventoryReport = parseTankInventoryReport;
            _inventarioService = inventarioService;
            _descargasService = descargasService;
            _portName = config["SerialPort:PortName"];
            _baudRate = int.Parse(config["SerialPort:BaudRate"]);
            _idEstacion = int.Parse(config["Estacion:Id"]);
        }


        public void ShowHeaderConnectionEstablished()
        {
            Console.WriteLine("\nTerminal de Comunicación Serial - Modo Interactivo");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Escribe 'help' para ver los comandos disponibles");
            Console.WriteLine("Escribe 'exit' para salir\n");

            ShowPrompt();
        }

        private void ProcessInputCommand(string input)
        {
            lock (_consoleLock)
            {
                switch (input.ToLower())
                {
                    case "help":
                        ShowHelp();
                        break;
                    case "clear":
                        Console.Clear();
                        ShowHeaderConnectionEstablished();
                        break;
                    default:
                        SendCommand(input);
                        break;
                }
            }
        }

        private void SendCommand(string command)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nEnviando comando: {command}");
            Console.ResetColor();

            _serialPortService.Write(command);

            ActionCommand(command);
        }

        private void ActionCommand(string command)
        {
            string commandPrefix = command[..4];
            // Remover el manejador previo si existe
            if (_currentResponseHandler != null)
            {
                _serialPortService.CompleteResponseReceived -= _currentResponseHandler;
            }

            switch (commandPrefix)
            {
                case "i201":
                    _currentResponseHandler = async (sender, response) =>
                    {
                        if (ValidationRequestEmply(response)) return;

                        TankReport result = _parseTankInventoryReport.Execute(response);
                        var date = result.Date;

                        foreach (var tank in result.Tanks)
                        {
                            InventarioEntity inventario = new(
                                _idEstacion,
                                tank.NoTank,
                                "",
                                tank.TankData.Volume,
                                tank.TankData.Temperature,
                                DateTime.Now
                            );

                            try
                            {
                                await _inventarioService.AddAsync(inventario);
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
                    break;

                case "i202":
                    _currentResponseHandler = async (sender, response) =>
                    {
                        if (ValidationRequestEmply(response)) return;

                        DeliveryTankReport result = _parceDeliveryReport.Execute(response);

                        foreach (var tank in result.Tanks)
                        {
                            var volumenInicial = tank.Deliveries.FirstOrDefault()?.Start.Volume ?? 0;
                            var volumenDisponible = tank.Deliveries.FirstOrDefault()?.End.Volume ?? 0;

                            DescargasEntity descarga = new(
                                _idEstacion,
                                tank.NoTank,
                                volumenInicial,
                                tank.Deliveries.FirstOrDefault()?.Start.Temperature ?? 0,
                                tank.Deliveries.FirstOrDefault().Start.Date,
                                volumenDisponible,
                                tank.Deliveries.FirstOrDefault()?.End.Temperature ?? 0,
                                tank.Deliveries.FirstOrDefault().End.Date,
                                volumenDisponible - volumenInicial
                            );
                            
                            try
                            {
                                await _descargasService.AddAsync(descarga);
                                Console.WriteLine("Ultimos registros guardados");
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
                    break;
            }
        }

        public bool ValidationRequestEmply(string input)
        {
            string claveVacio = input.Trim('\u0003')[^4..][..2];

            if (claveVacio == "FC")
            {
                Console.WriteLine("Sin ningun resultado");
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Run()
        {
            try
            {
                bool connectionSuccess = AttemptInitialConnection();

                if (!connectionSuccess)
                {
                    InitializeSerialPort();
                }

                ShowHeaderConnectionEstablished();
                ProcessCommands();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError crítico: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                _serialPortService.Close();
                Console.WriteLine("\nAplicación terminada");
            }
        }

        private bool AttemptInitialConnection()
        {
            try
            {
                Console.WriteLine($"\nIntentando conexión con {_portName} @ {_baudRate} baud...");
                _serialPortService.Initialize(_portName, _baudRate);
                _serialPortService.DataReceived += OnDataReceived;
                _serialPortService.CompleteResponseReceived += OnCompleteResponseReceived;
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError al conectar: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        private void ProcessCommands()
        {
            while (true)
            {
                var input = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                ProcessInputCommand(input);
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("\nCOMANDOS DISPONIBLES:");
            Console.WriteLine("---------------------");
            Console.WriteLine("I201XX - Muestra inventario de los tanques (ej. 00 - todos, 0X - tanque X)");
            Console.WriteLine("I202XX - Muestra los últimos 10 cargas a los tanques (ej. 00 - todos, 0X - tanque X)");
            Console.WriteLine("clean - Limpia la consola");
            Console.WriteLine("exit - Salir del programa\n");
        }

        private void InitializeSerialPort()
        {
            bool portInitialized = false;
            string portName = _portName;
            int baudRate = _baudRate;

            while (!portInitialized)
            {
                try
                {
                    Console.Clear();
                    ShowAvailablePorts();

                    Console.WriteLine($"\nConfiguración actual: {portName} @ {baudRate} baud");
                    Console.WriteLine("¿Qué deseas hacer?");
                    Console.WriteLine("1. Conectar con la configuración actual");
                    Console.WriteLine("2. Cambiar puerto COM");
                    Console.WriteLine("3. Cambiar baud rate");
                    Console.WriteLine("4. Detectar puertos automáticamente");
                    Console.WriteLine("5. Salir");
                    Console.Write("Seleccione una opción: ");

                    var option = Console.ReadLine();
                    switch (option)
                    {
                        case "1": // Intentar conectar
                            AttemptConnection(portName, baudRate, ref portInitialized);
                            break;
                        case "2": // Cambiar puerto
                            portName = SelectComPort();
                            break;
                        case "3": // Cambiar baud rate
                            baudRate = SelectBaudRate();
                            break;
                        case "4": // Detección automática
                            (portName, baudRate) = TryAutoDetectPort();
                            break;
                        case "5":
                            Environment.Exit(0);
                            break;
                        default:
                            Console.WriteLine("Opción no válida");
                            Thread.Sleep(1000);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nError: {ex.Message}");
                    Console.ResetColor();
                    Console.WriteLine("Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }

        private void AttemptConnection(string portName, int baudRate, ref bool portInitialized)
        {
            Console.WriteLine($"\nConectando a {portName} @ {baudRate} baud...");
            _serialPortService.Initialize(portName, baudRate);

            _serialPortService.DataReceived += OnDataReceived;
            _serialPortService.CompleteResponseReceived += OnCompleteResponseReceived;

            _portName = portName;
            _baudRate = baudRate;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"¡Conexión exitosa con {portName}!");
            Console.ResetColor();

            portInitialized = true;
            Thread.Sleep(1000);
        }

        private static string SelectComPort()
        {
            Console.Write("\nIngrese el nombre del puerto COM (ej. COM4): ");
            string port = Console.ReadLine().Trim().ToUpper();

            // Validación básica
            if (!port.StartsWith("COM") || !int.TryParse(port[3..], out _))
            {
                throw new ArgumentException("Formato de puerto inválido. Debe ser COM seguido de un número.");
            }

            return port;
        }

        private static int SelectBaudRate()
        {
            Console.Write("\nIngrese el baud rate (ej. 9600): ");

            if (!int.TryParse(Console.ReadLine(), out int baudRate) || baudRate <= 0)
                throw new ArgumentException("Baud rate debe ser un número positivo.");

            return baudRate;
        }

        private (string port, int baudRate) TryAutoDetectPort()
        {
            string[] availablePorts = SerialPort.GetPortNames();

            if (availablePorts.Length == 0)
                throw new Exception("No se detectaron puertos COM disponibles");

            Console.WriteLine("\nProbando puertos disponibles...");

            foreach (string port in availablePorts)
            {
                Console.Write($"\nProbando {port}... ");

                try
                {
                    using var testPort = new SerialPort(port, _baudRate);

                    testPort.Open();
                    testPort.Close();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("OK");
                    Console.ResetColor();

                    return (port, _baudRate);
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error");
                    Console.ResetColor();
                }
            }

            throw new Exception("No se pudo conectar a ningún puerto automáticamente");
        }

        private static void ShowAvailablePorts()
        {
            string[] ports = SerialPort.GetPortNames();

            Console.WriteLine("═══════════════════════════════════");
            Console.WriteLine("  ADMINISTRADOR DE PUERTOS COM");
            Console.WriteLine("═══════════════════════════════════");

            if (ports.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" No se detectaron puertos COM");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(" Puertos disponibles:");
                foreach (string port in ports)
                {
                    Console.WriteLine($" - {port}");
                }
                Console.ResetColor();
            }
        }

        private void OnDataReceived(object sender, string data)
        {
            lock (_consoleLock)
            {
                if (!_responseStopwatch.IsRunning)
                    _responseStopwatch.Start(); // Inicia el cronómetro al primer dato recibido

                _isReceivingData = true;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(data); // Muestra datos en tiempo real
                Console.ResetColor();
            }
        }

        private void OnCompleteResponseReceived(object sender, string completeResponse)
        {
            lock (_consoleLock)
            {
                _isReceivingData = false;
                _responseStopwatch.Stop(); // Detiene el cronómetro al recibir la respuesta completa

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[Respuesta completa en {_responseStopwatch.Elapsed.TotalSeconds:0.000} segundos]");
                Console.ResetColor();

                Console.WriteLine($"[Tamaño: {completeResponse.Length} caracteres]");
                Console.ResetColor();
                
                _responseStopwatch.Reset();
                ShowPrompt();
            }
        }

        private void ShowPrompt()
        {
            if (!_isReceivingData)
            {
                Console.Write("\n>> ");
                Console.Out.Flush(); // Asegura que el prompt se muestre inmediatamente
            }
        }
    }
}