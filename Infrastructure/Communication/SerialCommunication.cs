using Domain.Interfaces;
using System.IO.Ports;

namespace Infrastructure.Communication
{
    public class SerialCommunication : ISerialCommunication
    {
        private readonly SerialPort _serialPort;
        public bool IsOpen => _serialPort.IsOpen;
        public SerialCommunication(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate)
            {
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500
            };
        }
        public void Open()
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
                Console.WriteLine($"Conexion establecida con {_serialPort.PortName}");
            }
        }
        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                Console.WriteLine("Conexion cerrada");
            }
        }
        public string Read()
        {
            Thread.Sleep(5000);
            if (_serialPort.BytesToRead > 0)
                return $"Respuesta recibida: {_serialPort.ReadExisting()}";
            else
                return "No se recibio respuesta.";
        }
        public void Write(string message)
        {
            _serialPort.WriteLine($"\x01{message}");
        }
    }
}
