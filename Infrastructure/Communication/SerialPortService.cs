using Application.Interfaces;
using System.IO.Ports;
using System.Text;

namespace Infrastructure.Communication
{
    public class SerialPortService : ISerialPortService
    {
        private SerialPort _serialPort;
        private readonly object _lock = new();
        private bool _initialized = false;
        private bool _disposed = false;
        private readonly StringBuilder _receivedDataBuffer = new();
        private Timer _inactivityTimer;
        private const int InactivityTimeout = 300; // ms sin datos para considerar fin de transmisión

        public event EventHandler<string> DataReceived;
        public event EventHandler<string> CompleteResponseReceived;

        public bool IsOpen
        {
            get
            {
                lock (_lock)
                {
                    return _serialPort != null && _serialPort.IsOpen;
                }
            }
        }

        public void Initialize(string portName, int baudRate)
        {
            //if (_disposed)
            //    throw new ObjectDisposedException("El puerto serial ha sido cerrado");

            lock (_lock)
            {
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    _serialPort.Dispose();
                    _serialPort = null;
                    _initialized = false;
                    _disposed = false;
                }

                try
                {
                    Console.WriteLine($"Intentando inicializar puerto {portName}...");

                    _serialPort = new SerialPort(portName, baudRate)
                    {
                        Handshake = Handshake.None
                    };

                    _serialPort.DataReceived += SerialPort_DataReceived;
                    _inactivityTimer = new Timer(InactivityTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);

                    _serialPort.Open();
                    _initialized = true;

                    Console.WriteLine($"Puerto {portName} inicializado correctamente");
                    Console.WriteLine($"Estado: {_serialPort.IsOpen}");
                    Console.WriteLine($"Configuración: {baudRate} baud, {_serialPort.Parity}, {_serialPort.DataBits} bits de datos, {_serialPort.StopBits} bits de parada");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error durante inicialización: {ex.Message}");
                    _initialized = false;
                    throw;
                }
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (_lock)
            {
                try
                {
                    string newData = _serialPort.ReadExisting();
                    if (!string.IsNullOrEmpty(newData))
                    {
                        // Reinicia el timer de inactividad
                        _inactivityTimer.Change(InactivityTimeout, Timeout.Infinite);

                        _receivedDataBuffer.Append(newData);

                        // Notifica los datos recibidos en tiempo real
                        DataReceived?.Invoke(this, newData);

                        // Verifica si tenemos un ETX (fin de mensaje)
                        if (newData.Contains('\u0003'))
                        {
                            ProcessCompleteResponse();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en recepción de datos: {ex.Message}");
                }
            }
        }

        private void InactivityTimerElapsed(object state)
        {
            lock (_lock)
            {
                if (_receivedDataBuffer.Length > 0)
                {
                    ProcessCompleteResponse();
                }
            }
        }

        private void ProcessCompleteResponse()
        {
            // Detiene el timer temporalmente
            _inactivityTimer.Change(Timeout.Infinite, Timeout.Infinite);

            string completeResponse = _receivedDataBuffer.ToString();
            _receivedDataBuffer.Clear();

            // Notifica la respuesta completa
            CompleteResponseReceived?.Invoke(this, completeResponse);
        }

        public void Write(string data)
        {
            //if (_disposed)
            //    throw new ObjectDisposedException("El puerto serial ha sido cerrado");

            lock (_lock)
            {
                try
                {
                    if (!_initialized || _serialPort == null)
                    {
                        throw new InvalidOperationException("El puerto serial no ha sido inicializado. Llame a Initialize() primero.");
                    }

                    if (!_serialPort.IsOpen)
                    {
                        throw new InvalidOperationException("El puerto serial no está abierto");
                    }

                    string framedData = $"\x01{data}"; // Añade SOH
                    _serialPort.Write(framedData);
                    Console.WriteLine("Comando enviado exitosamente");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error crítico en Write: {ex.Message}");
                    throw;
                }
            }
        }

        public void Close()
        {
            if (!_disposed)
            {
                lock (_lock)
                {
                    if (_serialPort?.IsOpen == true)
                    {
                        _serialPort.Close();
                        Console.WriteLine("Puerto serial cerrado");
                    }
                    _inactivityTimer?.Dispose();
                    _serialPort?.Dispose();
                    _disposed = true;
                }
            }
        }

        public void Dispose() => Close();

        public void ClearBuffer()
        {
            lock (_lock)
            {
                _receivedDataBuffer.Clear();
            }
        }
    }
}