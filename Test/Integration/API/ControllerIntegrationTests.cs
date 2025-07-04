using Test.Fixtures;

namespace Test.Integration.API
{
    /// <summary>
    /// Pruebas de integración básicas sin dependencias complejas
    /// </summary>
    public class BasicIntegrationTests
    {
        [Fact]
        public void InventarioEntity_CanBeCreatedInIntegrationContext()
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
        }

        [Fact]
        public void DescargasEntity_CanBeCreatedInIntegrationContext()
        {
            // Arrange & Act
            var descarga = new DescargaEntityBuilder()
                .WithIdEstacion(11162)
                .WithNoTanque("1")
                .Build();

            // Assert
            descarga.Should().NotBeNull();
            descarga.IdEstacion.Should().Be(11162);
        }

        [Theory]
        [InlineData(11162, "1")]
        [InlineData(11163, "2")]
        [InlineData(11164, "3")]
        public void EntityBuilders_ShouldWorkWithDifferentValues(int estacion, string tanque)
        {
            // Arrange & Act
            var inventario = new InventarioEntityBuilder()
                .WithIdEstacion(estacion)
                .WithNoTanque(tanque)
                .Build();

            var descarga = new DescargaEntityBuilder()
                .WithIdEstacion(estacion)
                .WithNoTanque(tanque)
                .Build();

            // Assert
            inventario.IdEstacion.Should().Be(estacion);
            inventario.NoTanque.Should().Be(int.Parse(tanque));
            descarga.IdEstacion.Should().Be(estacion);
            descarga.NoTanque.Should().Be(int.Parse(tanque));
        }

        [Fact]
        public void MultipleEntities_CanBeCreatedTogether()
        {
            // Arrange & Act
            var inventarios = new List<Domain.Entities.InventarioEntity>();
            var descargas = new List<Domain.Entities.DescargasEntity>();

            for (int i = 1; i <= 3; i++)
            {
                inventarios.Add(new InventarioEntityBuilder()
                    .WithNoTanque(i.ToString())
                    .WithVolumenDisponible(15000.0 + (i * 1000))
                    .Build());

                descargas.Add(new DescargaEntityBuilder()
                    .WithNoTanque(i.ToString())
                    .WithCantidadCargada(10000.0 + (i * 500))
                    .Build());
            }

            // Assert
            inventarios.Should().HaveCount(3);
            descargas.Should().HaveCount(3);
            
            for (int i = 0; i < 3; i++)
            {
                inventarios[i].NoTanque.Should().Be(i + 1);
                descargas[i].NoTanque.Should().Be(i + 1);
            }
        }
    }
}