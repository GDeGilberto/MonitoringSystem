using Domain.Interfaces;
using System.Diagnostics;
using System.IO.Ports;

namespace Presentation
{
    public class TerminalConsole
    {
        private readonly ISerialPortService _serialPortService;
        private bool _isReceivingData = false;
        private readonly object _consoleLock = new();
        private DateTime _lastDataReceivedTime = DateTime.MinValue;
        private readonly Stopwatch _responseStopwatch = new();
        private static string _lastSuccessfulPort = "COM3";
        private static int _lastSuccessfulBaudRate = 2400;

        public TerminalConsole(ISerialPortService serialPortService)
        {
            _serialPortService = serialPortService;
            InitializeSerialPort();
        }

        public void Run()
        {
            try
            {
                Console.WriteLine("\nTerminal de Comunicación Serial - Modo Interactivo");
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("Los datos recibidos se muestran en tiempo real");
                Console.WriteLine("Escribe 'exit' para salir\n");

                ShowPrompt();

                while (true)
                {
                    var input = Console.ReadLine();
                    if (input?.ToLower() == "exit") break;

                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        lock (_consoleLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"\nEnviando: {input}");
                            Console.ResetColor();
                            _serialPortService.Write(input);
                        }
                    }
                }
            }
            finally
            {
                _serialPortService.Close();
                Console.WriteLine("\nAplicación terminada");
            }
        }

        private void InitializeSerialPort()
        {
            bool portInitialized = false;
            string portName = _lastSuccessfulPort;
            int baudRate = _lastSuccessfulBaudRate;

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

            _lastSuccessfulPort = portName;
            _lastSuccessfulBaudRate = baudRate;

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
            {
                throw new ArgumentException("Baud rate debe ser un número positivo.");
            }

            return baudRate;
        }

        private static (string port, int baudRate) TryAutoDetectPort()
        {
            string[] availablePorts = SerialPort.GetPortNames();
            if (availablePorts.Length == 0)
            {
                throw new Exception("No se detectaron puertos COM disponibles");
            }

            Console.WriteLine("\nProbando puertos disponibles...");

            foreach (string port in availablePorts)
            {
                Console.Write($"\nProbando {port}... ");

                try
                {
                    using var testPort = new SerialPort(port, _lastSuccessfulBaudRate)
                    {
                        ReadTimeout = 500,
                        WriteTimeout = 500
                    };

                    testPort.Open();
                    testPort.Close();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("OK");
                    Console.ResetColor();

                    return (port, _lastSuccessfulBaudRate);
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
                _lastDataReceivedTime = DateTime.Now;

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