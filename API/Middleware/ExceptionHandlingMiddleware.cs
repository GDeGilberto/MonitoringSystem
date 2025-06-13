using API.Models;
using Infrastructure.Repositories;
using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    /// <summary>
    /// Middleware for handling exceptions globally across the API
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next, 
            ILogger<ExceptionHandlingMiddleware> logger, 
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred during request processing");

            HttpStatusCode statusCode;
            ErrorResponse errorResponse;

            // Map each exception type to appropriate HTTP status code and response format
            switch (exception)
            {
                case EntityNotFoundException notFoundEx:
                    statusCode = HttpStatusCode.NotFound;
                    errorResponse = ErrorResponse.NotFound(notFoundEx.Message);
                    break;

                case DuplicateEntityException duplicateEx:
                    statusCode = HttpStatusCode.Conflict;
                    errorResponse = ErrorResponse.Conflict(duplicateEx.Message);
                    break;

                case DatabaseConnectionException dbEx:
                    statusCode = HttpStatusCode.ServiceUnavailable;
                    errorResponse = ErrorResponse.ServiceUnavailable(
                        "Error de conexión a la base de datos", 
                        _env.IsDevelopment() ? dbEx.Message : null);
                    break;

                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = ErrorResponse.ValidationError(
                        "Datos inválidos en la solicitud",
                        new[] { argEx.Message });
                    break;

                case RepositoryException repoEx:
                    statusCode = HttpStatusCode.InternalServerError;
                    errorResponse = ErrorResponse.FromException(
                        repoEx,
                        "https://tools.ietf.org/html/rfc7231#section-6.6.1");
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    errorResponse = _env.IsDevelopment()
                        ? ErrorResponse.FromException(exception)
                        : new ErrorResponse(
                            "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                            "Ocurrió un error interno al procesar la solicitud");
                    break;
            }

            // Set content type and status code
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            // Serialize and write the error response
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
        }
    }
}