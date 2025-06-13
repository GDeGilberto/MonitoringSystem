namespace API.Middleware
{
    /// <summary>
    /// Extension methods for adding custom middlewares to the application pipeline
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds the global exception handling middleware to the application pipeline
        /// </summary>
        /// <param name="app">The application builder instance</param>
        /// <returns>The application builder instance</returns>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}