using Hangfire;

namespace API.Extensions
{
    public static class HangfireExtensions
    {
        public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Hangfire with SQL Server storage
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));
            
            // Add Hangfire server
            services.AddHangfireServer();
            
            return services;
        }
        
        public static IApplicationBuilder UseHangfireDashboardWithSecurity(this IApplicationBuilder app)
        {
            // Can be extended with authentication options if needed
            app.UseHangfireDashboard();
            
            return app;
        }
    }
}