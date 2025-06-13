using Hangfire;
using Infrastructure.Jobs;

namespace API.Extensions
{
    public static class JobsExtensions
    {
        public static void ConfigureRecurringJobs(this IApplicationBuilder app)
        {
            // Tank inventory monitoring job - runs every 3 minutes
            RecurringJob.AddOrUpdate<InventarioJob>(
                "job-volumenYTemperatura-de-tanques-3-minutos",
                job => job.Execute(),
                "*/3 * * * *");
            
            // Tank delivery job - runs daily at 6:00 AM
            RecurringJob.AddOrUpdate<DescargasJobs>(
                "job-Cargas-a-los-tanques",
                job => job.Execute(),
                Cron.Daily(6));
        }
    }
}