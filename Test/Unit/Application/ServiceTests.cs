using Application.Services;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ViewModels;
using Test.Fixtures;

namespace Test.Unit.Application
{
    /// <summary>
    /// Pruebas unitarias básicas para servicios
    /// </summary>
    public class BasicServiceTests
    {
        [Fact]
        public void InventarioService_CanBeCreated()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<InventarioEntity>>();
            var mockPresenter = new Mock<IPresenter<InventarioEntity, InventarioViewModel>>();

            // Act
            var service = new InventarioService<InventarioEntity, InventarioViewModel>(
                mockRepository.Object, 
                mockPresenter.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public async Task InventarioService_AddAsync_ShouldCallRepository()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<InventarioEntity>>();
            var mockPresenter = new Mock<IPresenter<InventarioEntity, InventarioViewModel>>();
            var service = new InventarioService<InventarioEntity, InventarioViewModel>(
                mockRepository.Object, mockPresenter.Object);
            var inventario = new InventarioEntityBuilder().Build();

            // Act
            await service.AddAsync(inventario);

            // Assert
            mockRepository.Verify(r => r.AddAsync(inventario), Times.Once);
        }

        [Fact]
        public void DescargasService_CanBeCreated()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<DescargasEntity>>();

            // Act
            var service = new DescargasService<DescargasEntity>(mockRepository.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public async Task DescargasService_AddAsync_ShouldCallRepository()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<DescargasEntity>>();
            var service = new DescargasService<DescargasEntity>(mockRepository.Object);
            var descarga = new DescargaEntityBuilder().Build();

            // Act
            await service.AddAsync(descarga);

            // Assert
            mockRepository.Verify(r => r.AddAsync(descarga), Times.Once);
        }

        [Fact]
        public async Task DescargasService_AddAsync_WithNull_ShouldNotThrowException()
        {
            // Arrange
            var mockRepository = new Mock<IRepository<DescargasEntity>>();
            var service = new DescargasService<DescargasEntity>(mockRepository.Object);

            // Act & Assert - The service might not validate null, which is okay for this basic implementation
            await service.AddAsync(null!);
            
            // Verify the repository was called even with null
            mockRepository.Verify(r => r.AddAsync(null!), Times.Once);
        }

        [Fact]
        public void ServiceBuilders_ShouldCreateValidObjects()
        {
            // Arrange
            var mockInventarioRepository = new Mock<IRepository<InventarioEntity>>();
            var mockDescargasRepository = new Mock<IRepository<DescargasEntity>>();
            var mockPresenter = new Mock<IPresenter<InventarioEntity, InventarioViewModel>>();

            // Act
            var inventarioService = new InventarioService<InventarioEntity, InventarioViewModel>(
                mockInventarioRepository.Object, mockPresenter.Object);
            var descargasService = new DescargasService<DescargasEntity>(mockDescargasRepository.Object);

            // Assert
            inventarioService.Should().NotBeNull();
            descargasService.Should().NotBeNull();
        }
    }
}