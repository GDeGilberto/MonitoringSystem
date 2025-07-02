using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Infrastructure.Communication
{
    public class DagalSoapService : IDagalSoapService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DagalSoapService> _logger;
        private readonly string _soapEndpoint;
        private readonly string _username;
        private readonly string _password;

        public DagalSoapService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            ILogger<DagalSoapService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            // Obtener configuraciones del appsettings
            _soapEndpoint = _configuration["Dagal:Endpoint"] ?? throw new InvalidOperationException("Dagal:Endpoint no configurado");
            _username = _configuration["Dagal:Username"] ?? throw new InvalidOperationException("Dagal:Username no configurado");
            _password = _configuration["Dagal:Password"] ?? throw new InvalidOperationException("Dagal:Password no configurado");
        }

        public async Task<bool> RegistrarEstatusInventarioAsync(InventarioEntity inventario)
        {
            try
            {
                _logger.LogInformation("Enviando inventario a Dagal SOAP - Estación: {Estacion}, Tanque: {Tanque}", 
                    inventario.IdEstacion, inventario.NoTanque);

                var soapEnvelope = CreateSoapEnvelope(inventario);
                var content = new StringContent(soapEnvelope, Encoding.UTF8, "application/soap+xml");

                // Configurar headers
                content.Headers.Add("SOAPAction", "http://dagal.com/RegistrarEstatusUCCVPs2Activo");

                var response = await _httpClient.PostAsync(_soapEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Inventario enviado exitosamente a Dagal. Respuesta: {Response}", responseContent);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al enviar inventario a Dagal. Status: {Status}, Content: {Content}", 
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Error de comunicación al enviar inventario a Dagal");
                return false;
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(timeoutEx, "Timeout al enviar inventario a Dagal");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al enviar inventario a Dagal");
                return false;
            }
        }

        private string CreateSoapEnvelope(InventarioEntity inventario)
        {
            // Formatear el número de tanque con ceros a la izquierda si es necesario
            var noTanqueStr = inventario.NoTanque?.ToString("D2") ?? "00";
            
            // Obtener la clave del producto del tanque desde configuración o usar un valor por defecto
            var claveProducto = inventario.ClaveProducto ?? "34006"; // Valor por defecto si no está disponible
            
            // Formatear la fecha en formato ISO - verificar si es nullable
            var fechaFormatted = inventario.Fecha?.ToString("yyyy-MM-ddTHH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
  <soap12:Header>
    <AuthenticationHeader xmlns=""http://dagal.com/"">
      <UserName>{_username}</UserName>
      <Password>{_password}</Password>
    </AuthenticationHeader>
  </soap12:Header>
  <soap12:Body>
    <RegistrarEstatusUCCVPs2Activo xmlns=""http://dagal.com/"">
      <notanque>{noTanqueStr}</notanque>
      <claveProducto>{claveProducto}</claveProducto>
      <estacion>{inventario.IdEstacion}</estacion>
      <fecha>{fechaFormatted}</fecha>
      <volumen>{inventario.VolumenDisponible:F2}</volumen>
      <temperatura>{inventario.Temperatura:F1}</temperatura>
    </RegistrarEstatusUCCVPs2Activo>
  </soap12:Body>
</soap12:Envelope>";

            return soapEnvelope;
        }
    }
}