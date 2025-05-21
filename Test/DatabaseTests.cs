using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Test
{
    public class DatabaseTests
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;

        public DatabaseTests()
        {
            _connectionString = GetConnectionString();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(_connectionString)
                .Options;

            _context = new AppDbContext(options);
        }

        private string GetConnectionString()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                return config.GetConnectionString("DefaultConnection");
            }
            catch
            {
                // Fallback manual
                return "";
            }
        }

        [Fact(DisplayName = "CONEXIÓN - Prueba básica de conectividad")]
        public async Task CanConnectToDatabase()
        {
            var canConnect = await _context.Database.CanConnectAsync();

            Assert.True(canConnect);
            Console.WriteLine($"=== Conexión {canConnect} a la base de datos ===");
        }


        [Fact(DisplayName = "CONSULTA TABLA - Consulta completa de CatEstaciones")]
        public async Task ShouldRetrieveAllStations()
        {
            var stations = await _context.CatEstaciones.ToListAsync();

            Assert.NotNull(stations);
            Assert.NotEmpty(stations);
        }

        [Fact(DisplayName = "CONSULTA REGISTRO - Primer registro de CatEstaciones")]
        public async Task ShouldGetFirstStationRecord()
        {
            var firstStation = await _context.CatEstaciones
                .OrderBy(s => s.IdEstacion)
                .FirstOrDefaultAsync();

            Assert.NotNull(firstStation);
            Assert.True(firstStation.IdEstacion > 0);
            Console.WriteLine($"=== Registro obtenido: ID= {firstStation.IdEstacion}, Nombre= {firstStation.Nombre} ===");
        }

        [Fact(DisplayName = "NUEVO REGISTRO - Crear nuevo registro en Proc_Inventario")]
        public async Task ShouldCreateNewInventoryRecord()
        {
            // Arrange - Preparar datos de prueba
            var newRecord = new ProcInventario
            {
                Idestacion = 1945,
                NoTanque = "04",
                ClaveProducto = "30348",
                VolumenDisponible = 35000,
                Temperatura = 0,
                Fecha = DateTime.Now
            };


            // Act - Insertar el registro
            _context.ProcInventarios.Add(newRecord);
            var result = await _context.SaveChangesAsync();


            // Assert - Verificaciones
            Assert.Equal(1, result); // Se afectó 1 registro
            Console.WriteLine($"Nuevo registro creado con ID: {newRecord.IdReg}");

            // Opcional: Consultar el registro recién creado
            var insertedRecord = await _context.ProcInventarios
                .FirstOrDefaultAsync(p => p.IdReg == newRecord.IdReg);
            Assert.NotNull(insertedRecord);
            Assert.Equal(35000, insertedRecord.VolumenDisponible);
        }
    }
}
