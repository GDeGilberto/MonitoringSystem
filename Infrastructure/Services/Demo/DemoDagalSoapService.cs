using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Demo
{
    public class DemoDagalSoapService : IDagalSoapService
    {
        private readonly ILogger<DemoDagalSoapService> _logger;

        public DemoDagalSoapService(ILogger<DemoDagalSoapService> logger)
        {
            _logger = logger;
        }

        public Task<bool> RegistrarEstatusInventarioAsync(InventarioEntity inventario)
        {
            _logger.LogInformation("DEMO: Simulando envío de inventario - Estación: {Estacion}, Tanque: {Tanque}, Volumen: {Volumen}",
                inventario.IdEstacion, inventario.NoTanque, inventario.VolumenDisponible);

            // Simulate successful response
            return Task.FromResult(true);
        }

        public Task<bool> RegistrarEstatusDescargaAsync(DescargasEntity descarga)
        {
            _logger.LogInformation("DEMO: Simulando envío de descarga - Estación: {Estacion}, Tanque: {Tanque}, Cantidad: {Cantidad}",
                descarga.IdEstacion, descarga.NoTanque, descarga.CantidadCargada);

            // Simulate successful response
            return Task.FromResult(true);
        }

        public Task<string> TestConnectionAsync()
        {
            _logger.LogInformation("DEMO: Simulando prueba de conexión SOAP");
            return Task.FromResult("Demo connection successful");
        }
    }
}