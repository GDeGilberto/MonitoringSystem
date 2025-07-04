using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Infrastructure.Data;

namespace Test.Configuration
{
    /// <summary>
    /// Configuración simplificada para las pruebas
    /// </summary>
    public class TestConfiguration
    {
        public static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            
            // Configuration
            var configuration = CreateTestConfiguration();
            services.AddSingleton<IConfiguration>(configuration);
            
            // Logging
            services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Information));
            
            // Database - In Memory for tests
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            
            return services.BuildServiceProvider();
        }
        
        public static IConfiguration CreateTestConfiguration()
        {
            var configData = new Dictionary<string, string?>
            {
                ["Estacion:Id"] = "11162",
                ["SerialPort:PortName"] = "COM1",
                ["SerialPort:BaudRate"] = "9600",
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:"
            };
            
            return new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }
        
        public static AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            
            return new AppDbContext(options);
        }
    }
}