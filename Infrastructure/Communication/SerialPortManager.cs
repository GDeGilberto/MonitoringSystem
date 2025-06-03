using Application.Interfaces;
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
            await _semaphore.WaitAsync();
            try
            {
                Initialize(portName, baudRate);

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
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
                _initialized = false;
                _disposed = false;
            }

            _serialPort = new SerialPort(portName, baudRate)
            {
                Handshake = Handshake.None
            };
            _serialPort.DataReceived += SerialPort_DataReceived;
            _inactivityTimer = new Timer(InactivityTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            _serialPort.Open();
            _initialized = true;
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
                throw new InvalidOperationException("El puerto serial no está abierto");

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