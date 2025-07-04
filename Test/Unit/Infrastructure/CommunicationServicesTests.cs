using Infrastructure.Communication.Demo;
using Microsoft.Extensions.Logging;
using Test.Fixtures;

namespace Test.Unit.Infrastructure
{
    /// <summary>
    /// Pruebas básicas para servicios de comunicación
    /// </summary>
    public class BasicCommunicationTests
    {
        [Fact]
        public void DemoSerialPortService_CanBeCreated()
        {
            // Arrange & Act
            var service = new DemoSerialPortService();

            // Assert
            service.Should().NotBeNull();
            service.IsOpen.Should().BeFalse();
        }

        [Fact]
        public void DemoSerialPortService_Initialize_ShouldSetIsOpen()
        {
            // Arrange
            var service = new DemoSerialPortService();

            // Act
            service.Initialize("COM1", 9600);

            // Assert
            service.IsOpen.Should().BeTrue();
        }

        [Fact]
        public void DemoSerialPortService_Close_ShouldSetIsOpenFalse()
        {
            // Arrange
            var service = new DemoSerialPortService();
            service.Initialize("COM1", 9600);

            // Act
            service.Close();

            // Assert
            service.IsOpen.Should().BeFalse();
        }

        [Fact]
        public async Task DemoSerialPortService_Write_ShouldTriggerEvent()
        {
            // Arrange
            var service = new DemoSerialPortService();
            string? receivedData = null;
            service.DataReceived += (sender, data) => receivedData = data;

            // Act
            service.Write("i20100");
            await Task.Delay(150); // Wait for async response

            // Assert
            receivedData.Should().NotBeNull();
            receivedData.Should().Contain("I20100");
        }
    }
}