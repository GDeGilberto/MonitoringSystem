using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        
        public ApiClientService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = _configuration["ApiKeys:JobKey"] ?? "";
            
            // Set base address if needed
            // _httpClient.BaseAddress = new Uri(configuration["Api:BaseUrl"]);
        }
        
        public async Task<T> GetAsync<T>(string endpoint)
        {
            AddApiKeyHeader();
            
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            
            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }
        
        public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            AddApiKeyHeader();
            
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            
            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }
        
        public async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            AddApiKeyHeader();
            
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = await _httpClient.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            
            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }
        
        private void AddApiKeyHeader()
        {
            _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
        }
    }
}