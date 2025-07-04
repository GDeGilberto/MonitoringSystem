using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Test.Configuration;
using Test.Fixtures;

namespace Test.Unit.Infrastructure
{
    /// <summary>
    /// Pruebas unitarias básicas para infraestructura
    /// </summary>
    public class BasicInfrastructureTests
    {
        [Fact]
        public void InventarioEntity_CanBeBuilt()
        {
            // Arrange & Act
            var inventario = new InventarioEntityBuilder()
                .WithIdEstacion(11162)
                .WithNoTanque("1")
                .WithVolumenDisponible(15000.0)
                .Build();

            // Assert
            inventario.Should().NotBeNull();
            inventario.IdEstacion.Should().Be(11162);
            inventario.NoTanque.Should().Be(1);
            inventario.VolumenDisponible.Should().Be(15000.0);
        }

        [Fact]
        public void DescargasEntity_CanBeBuilt()
        {
            // Arrange & Act
            var descarga = new DescargaEntityBuilder()
                .WithIdEstacion(11162)
                .WithNoTanque("2")
                .WithCantidadCargada(12000.0)
                .Build();

            // Assert
            descarga.Should().NotBeNull();
            descarga.IdEstacion.Should().Be(11162);
            descarga.NoTanque.Should().Be(2);
            descarga.CantidadCargada.Should().Be(12000.0);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("2", 2)]
        [InlineData("10", 10)]
        public void DescargaBuilder_WithStringTank_ShouldConvertToInt(string tankString, int expectedTank)
        {
            // Arrange & Act
            var descarga = new DescargaEntityBuilder()
                .WithNoTanque(tankString)
                .Build();

            // Assert
            descarga.NoTanque.Should().Be(expectedTank);
        }
    }
}