using Application.Interfaces;
using Infrastructure.Models;
using System.IO.Ports;
using System.Text;

namespace Infrastructure.Communication
{
    public class SerialPortManager : ISerialPortService
    {
        private SerialPort _serialPort;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly StringBuilder _receivedDataBuffer = new();
        private TaskCompletionSource<string> _responseTcs;
        private Timer _inactivityTimer;
        private const int InactivityTimeout = 300; // ms
        private bool _initialized = false;
        private bool _disposed = false;

        public event EventHandler<string> DataReceived;
        public event EventHandler<string> CompleteResponseReceived;

        public bool IsOpen
        {
            get
            {
                return _serialPort != null && _serialPort.IsOpen;
            }
        }

        public async Task<string> SendCommandAsync(string portName, int baudRate, string command, int timeoutMs = 3000)
        {
            var settings = new SerialPortSettings
            {
                PortName = portName,
                BaudRate = baudRate
            };
            return await SendCommandAsync(settings, command, timeoutMs);
        }

        public async Task<string> SendCommandAsync(SerialPortSettings settings, string command, int timeoutMs = 3000)
        {
            await _semaphore.WaitAsync();
            try
            {
                Initialize(settings);

                _responseTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                Write(command);

                using var cts = new CancellationTokenSource(timeoutMs);
                cts.Token.Register(() => _responseTcs.TrySetCanceled(), useSynchronizationContext: false);

                string response = await _responseTcs.Task;
                return response;
            }
            finally
            {
                Close();
                _semaphore.Release();
            }
        }

        public void Initialize(string portName, int baudRate)
        {
            var settings = new SerialPortSettings
            {
                PortName = portName,
                BaudRate = baudRate
            };
            Initialize(settings);
        }

        public void Initialize(SerialPortSettings settings)
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
                _initialized = false;
                _disposed = false;
            }

            Console.WriteLine($"SerialPortManager: Inicializando puerto {settings.PortName}...");
            Console.WriteLine($"Configuraci�n completa:");
            Console.WriteLine($"  - Puerto: {settings.PortName}");
            Console.WriteLine($"  - BaudRate: {settings.BaudRate}");
            Console.WriteLine($"  - DataBits: {settings.DataBits}");
            Console.WriteLine($"  - Parity: {settings.Parity}");
            Console.WriteLine($"  - StopBits: {settings.StopBits}");
            Console.WriteLine($"  - Handshake: {settings.Handshake}");

            _serialPort = new SerialPort(settings.PortName, settings.BaudRate)
            {
                DataBits = settings.DataBits,
                Parity = settings.GetParity(),
                StopBits = settings.GetStopBits(),
                Handshake = settings.GetHandshake(),
                ReadTimeout = settings.ReadTimeout,
                WriteTimeout = settings.WriteTimeout
            };

            _serialPort.DataReceived += SerialPort_DataReceived;
            _inactivityTimer = new Timer(InactivityTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            _serialPort.Open();
            _initialized = true;

            Console.WriteLine($"SerialPortManager: Puerto {settings.PortName} inicializado correctamente");
            Console.WriteLine($"Configuraci�n aplicada: {settings.BaudRate} baud, {_serialPort.Parity}, {_serialPort.DataBits} bits de datos, {_serialPort.StopBits} bits de parada, Handshake: {_serialPort.Handshake}");
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string newData = _serialPort.ReadExisting();
                if (!string.IsNullOrEmpty(newData))
                {
                    _inactivityTimer.Change(InactivityTimeout, Timeout.Infinite);
                    _receivedDataBuffer.Append(newData);
                    DataReceived?.Invoke(this, newData);

                    if (newData.Contains('\u0003'))
                    {
                        ProcessCompleteResponse();
                    }
                }
            }
            catch (Exception ex)
            {
                _responseTcs?.TrySetException(ex);
            }
        }

        private void InactivityTimerElapsed(object state)
        {
            if (_receivedDataBuffer.Length > 0)
            {
                ProcessCompleteResponse();
            }
        }

        private void ProcessCompleteResponse()
        {
            _inactivityTimer.Change(Timeout.Infinite, Timeout.Infinite);
            string completeResponse = _receivedDataBuffer.ToString();
            _receivedDataBuffer.Clear();
            CompleteResponseReceived?.Invoke(this, completeResponse);
            _responseTcs?.TrySetResult(completeResponse);
        }

        public void Write(string data)
        {
            if (!_initialized || _serialPort == null)
                throw new InvalidOperationException("El puerto serial no ha sido inicializado. Llame a Initialize() primero.");

            if (!_serialPort.IsOpen)
                throw new InvalidOperationException("El puerto serial no est� abierto");

            string framedData = $"\x01{data}";
            _serialPort.Write(framedData);
        }

        public void Close()
        {
            if (!_disposed)
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                }
                _inactivityTimer?.Dispose();
                _serialPort?.Dispose();
                _disposed = true;
            }
        }

        public void Dispose() => Close();

        public void ClearBuffer()
        {
            _receivedDataBuffer.Clear();
        }
    }
}