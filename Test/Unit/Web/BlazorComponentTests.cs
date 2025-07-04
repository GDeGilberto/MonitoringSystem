using Test.Fixtures;

namespace Test.Unit.Web
{
    /// <summary>
    /// Pruebas básicas para componentes web
    /// </summary>
    public class BasicWebTests
    {
        [Fact]
        public void InventarioEntity_CanBeUsedInWebContext()
        {
            // Arrange & Act
            var inventario = new InventarioEntityBuilder()
                .WithIdEstacion(11162)
                .WithNoTanque("1")
                .WithNombreProducto("Magna")
                .Build();

            // Assert
            inventario.Should().NotBeNull();
            inventario.IdEstacion.Should().Be(11162);
            inventario.ClaveProducto.Should().Be("01");
        }

        [Fact]
        public void DescargasEntity_CanBeUsedInWebContext()
        {
            // Arrange & Act
            var descarga = new DescargaEntityBuilder()
                .WithIdEstacion(11162)
                .WithNoTanque("1")
                .Build();

            // Assert
            descarga.Should().NotBeNull();
            descarga.IdEstacion.Should().Be(11162);
            descarga.NoTanque.Should().Be(1);
        }

        [Fact]
        public void EntityBuilders_ShouldCreateValidEntities()
        {
            // Arrange & Act
            var inventario = new InventarioEntityBuilder().Build();
            var descarga = new DescargaEntityBuilder().Build();

            // Assert
            inventario.Should().NotBeNull();
            descarga.Should().NotBeNull();
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("2", 2)]
        [InlineData("3", 3)]
        public void EntityBuilder_WithDifferentTankNumbers_ShouldWork(string tankString, int expectedTank)
        {
            // Arrange & Act
            var inventario = new InventarioEntityBuilder()
                .WithNoTanque(tankString)
                .Build();

            var descarga = new DescargaEntityBuilder()
                .WithNoTanque(tankString)
                .Build();

            // Assert
            inventario.NoTanque.Should().Be(expectedTank);
            descarga.NoTanque.Should().Be(expectedTank);
        }

        [Fact]
        public void BlazorComponents_RequiredDataTypes_ShouldExist()
        {
            // This test verifies that the basic data types needed for Blazor components exist
            
            // Arrange & Act
            var inventario = new InventarioEntityBuilder()
                .WithIdEstacion(11162)
                .WithVolumenDisponible(15000.0)
                .WithTemperatura(25.5)
                .Build();

            // Assert
            inventario.IdEstacion.Should().HaveValue();
            inventario.VolumenDisponible.Should().HaveValue();
            inventario.Temperatura.Should().HaveValue();
        }
    }
}