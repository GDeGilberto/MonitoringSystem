using Application.Interfaces;

namespace Infrastructure.Communication.Demo
{
    public class DemoSerialPortService : ISerialPortService
    {
        private readonly Random _random = new();
        private bool _isOpen = false;

        public event EventHandler<string>? DataReceived;
        public event EventHandler<string>? CompleteResponseReceived;

        public bool IsOpen => _isOpen;

        public void Initialize(string portName, int baudRate)
        {
            Console.WriteLine($"DEMO: Inicializando puerto serial simulado - Puerto: {portName}, BaudRate: {baudRate}");
            _isOpen = true;
        }

        public void Write(string data)
        {
            Console.WriteLine($"DEMO: Enviando comando simulado: {data}");
            
            // Simulate response after a short delay
            Task.Delay(100).ContinueWith(_ =>
            {
                var mockResponse = GenerateMockTankData();
                DataReceived?.Invoke(this, mockResponse);
                CompleteResponseReceived?.Invoke(this, mockResponse);
            });
        }

        public void Close()
        {
            Console.WriteLine("DEMO: Cerrando puerto serial simulado");
            _isOpen = false;
        }

        public void ClearBuffer()
        {
            Console.WriteLine("DEMO: Limpiando buffer simulado");
        }

        public void Dispose()
        {
            Close();
        }

        private string GenerateMockTankData()
        {
            var now = DateTime.Now;
            var report = $"I20100\r\n"; // Report header
            report += $"{now:MM/dd/yy}\r\n"; // Date
            report += $"{now:HH:mm:ss}\r\n"; // Time
            
            // Generate data for 4 tanks
            for (int i = 1; i <= 4; i++)
            {
                var volume = _random.Next(5000, 15000) + (_random.NextDouble() * 999);
                var temperature = 20 + (_random.NextDouble() * 10); // 20-30°C
                var water = _random.NextDouble() * 5; // 0-5mm water
                
                report += $"T{i:D2}\r\n"; // Tank number
                report += $"{volume:F2}\r\n"; // Volume
                report += $"{temperature:F1}\r\n"; // Temperature
                report += $"{water:F1}\r\n"; // Water level
                report += $"0\r\n"; // Status
            }
            
            return report;
        }
    }
}