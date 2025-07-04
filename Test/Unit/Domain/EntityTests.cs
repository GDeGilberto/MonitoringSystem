using Domain.Entities;
using Test.Fixtures;

namespace Test.Unit.Domain
{
    /// <summary>
    /// Pruebas unitarias básicas para las entidades del dominio
    /// </summary>
    public class BasicEntityTests
    {
        [Fact]
        public void InventarioEntity_CanBeCreated()
        {
            // Arrange & Act
            var inventario = new InventarioEntityBuilder().Build();

            // Assert
            inventario.Should().NotBeNull();
            inventario.IdEstacion.Should().Be(11162);
            inventario.NoTanque.Should().Be(1);
            inventario.ClaveProducto.Should().Be("01");
        }

        [Fact]
        public void InventarioEntity_WithNullValues_ShouldAcceptNullValues()
        {
            // Arrange & Act
            var inventario = new InventarioEntity(null, null, null, null, null, null);

            // Assert
            inventario.Should().NotBeNull();
            inventario.IdEstacion.Should().BeNull();
            inventario.NoTanque.Should().BeNull();
            inventario.ClaveProducto.Should().BeNull();
        }

        [Fact]
        public void DescargasEntity_CanBeCreated()
        {
            // Arrange & Act
            var descarga = new DescargaEntityBuilder().Build();

            // Assert
            descarga.Should().NotBeNull();
            descarga.IdEstacion.Should().Be(11162);
            descarga.NoTanque.Should().Be(1);
        }

        [Theory]
        [InlineData("Magna", "01")]
        [InlineData("Premium", "02")]
        [InlineData("Diesel", "03")]
        public void InventarioBuilder_ShouldMapProductNames(string productName, string expectedCode)
        {
            // Arrange & Act
            var inventario = new InventarioEntityBuilder()
                .WithNombreProducto(productName)
                .Build();

            // Assert
            inventario.ClaveProducto.Should().Be(expectedCode);
        }
    }
}