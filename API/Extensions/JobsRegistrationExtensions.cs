using Infrastructure.Jobs;
using Application.UseCases;

namespace API.Extensions
{
    public static class JobsRegistrationExtensions
    {
        public static IServiceCollection AddJobServices(this IServiceCollection services)
        {
            // Register job-related services
            services.AddScoped<ParceDeliveryReport>();
            services.AddScoped<ParseTankInventoryReport>();
            services.AddScoped<InventarioJob>();
            services.AddScoped<DescargasJobs>();
            
            return services;
        }
    }
}