using Application.UseCases;
using Application.Interfaces;
using Domain.Entities;
using Test.Fixtures;

namespace Test.Unit.Application
{
    /// <summary>
    /// Pruebas básicas para Use Cases
    /// </summary>
    public class BasicUseCaseTests
    {
        [Fact]
        public void GetInventariosUseCase_CanBeCreated()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<InventarioEntity>>();

            // Act
            var useCase = new GetInventariosUseCase(mockRepository.Object);

            // Assert
            useCase.Should().NotBeNull();
        }

        [Fact]
        public async Task GetInventariosUseCase_ExecuteAsync_ShouldCallRepository()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<InventarioEntity>>();
            var useCase = new GetInventariosUseCase(mockRepository.Object);
            var inventarios = new List<InventarioEntity>
            {
                new InventarioEntityBuilder().Build()
            };
            mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(inventarios);

            // Act
            var result = await useCase.ExecuteAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public void GetDescargasUseCase_CanBeCreated()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<DescargasEntity>>();

            // Act
            var useCase = new GetDescargasUseCase(mockRepository.Object);

            // Assert
            useCase.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDescargasUseCase_ExecuteAsync_ShouldCallRepository()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<DescargasEntity>>();
            var useCase = new GetDescargasUseCase(mockRepository.Object);
            var descargas = new List<DescargasEntity>
            {
                new DescargaEntityBuilder().Build()
            };
            mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(descargas);

            // Act
            var result = await useCase.ExecuteAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public void ParceDeliveryReport_CanBeCreated()
        {
            // Arrange & Act
            var parser = new ParceDeliveryReport();

            // Assert
            parser.Should().NotBeNull();
        }

        [Fact]
        public void ParseTankInventoryReport_CanBeCreated()
        {
            // Arrange & Act
            var parser = new ParseTankInventoryReport();

            // Assert
            parser.Should().NotBeNull();
        }
    }
}