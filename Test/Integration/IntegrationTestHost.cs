using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Test.Fixtures;

namespace Test.Integration
{
    /// <summary>
    /// Configuración para pruebas de integración completas
    /// </summary>
    public class IntegrationTestHost
    {
        public static IHost CreateHost()
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Add test database
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase("IntegrationTestDb"));

                    // Add logging
                    services.AddLogging(builder => 
                        builder.AddDebug().SetMinimumLevel(LogLevel.Information));

                    // Add test-specific services here
                });

            return hostBuilder.Build();
        }
    }

    /// <summary>
    /// Pruebas de integración end-to-end simplificadas
    /// </summary>
    public class BasicEndToEndTests
    {
        [Fact]
        public void EntitiesCanBeCreatedTogether()
        {
            // Arrange & Act
            var inventario = new InventarioEntityBuilder().Build();
            var descarga = new DescargaEntityBuilder().Build();

            // Assert
            inventario.Should().NotBeNull();
            descarga.Should().NotBeNull();
        }

        [Fact]
        public void BuildersProduceValidEntities()
        {
            // Arrange & Act
            var inventario = new InventarioEntityBuilder()
                .WithIdEstacion(11162)
                .WithNoTanque("1")
                .WithVolumenDisponible(15000.0)
                .WithTemperatura(25.5)
                .Build();

            var descarga = new DescargaEntityBuilder()
                .WithIdEstacion(11162)
                .WithNoTanque("1")
                .WithCantidadCargada(12000.0)
                .Build();

            // Assert
            inventario.IdEstacion.Should().Be(descarga.IdEstacion);
            inventario.NoTanque.Should().Be(descarga.NoTanque);
        }
    }
}