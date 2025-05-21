using Domain.Interfaces;
using Infrastructure.Communication;
using Newtonsoft.Json.Bson;

namespace Test
{
    public class SerialCommunicationTests
    {
        private readonly SerialCommunication _serial;
        private const string TestPort = "COM3"; // Cambiar por tu puerto
        private const int BaudRate = 2400;

        public SerialCommunicationTests()
        {
            _serial = new SerialCommunication(TestPort, BaudRate);
        }

        [Fact(DisplayName = "SERIAL CONEXION - Debe conectar con dispositivo real")]
        public void ConnectToRealDevice()
        {
            // Act
            _serial.Open();

            // Assert
            Assert.True(_serial.IsOpen);

            // Limpieza
            _serial.Close();
        }

        [Fact(DisplayName = "RESPUESTA DE COMANDO - Debe responder a un comando")]
        public void SendCommandAndReceiveResponse()
        {
            // Arrange
            string command = "I20100";

            // Act
            _serial.Open();
            _serial.Write(command);
            string response = _serial.Read();

            // Assert
            Assert.NotNull(response);

            // Limpieza
            _serial.Close();
        }
    }
}
