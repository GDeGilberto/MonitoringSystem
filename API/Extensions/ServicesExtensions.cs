using Application.Interfaces;
using Infrastructure.Services;

namespace API.Extensions
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddHttpClientServices(this IServiceCollection services)
        {
            // Register the HttpClientFactory and the ApiClientService
            services.AddHttpClient<IApiClientService, ApiClientService>();
            
            return services;
        }
    }
}