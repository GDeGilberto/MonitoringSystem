namespace API.Models
{
    /// <summary>
    /// Model for standardized error responses
    /// </summary>
    public class ErrorResponse
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Detail { get; set; }
        public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();
        public string TraceId { get; set; } = string.Empty;

        public ErrorResponse(string type, string message, string? detail = null, IEnumerable<string>? errors = null)
        {
            Type = type;
            Message = message;
            Detail = detail;
            Errors = errors ?? Array.Empty<string>();
            TraceId = Guid.NewGuid().ToString();
        }

        public static ErrorResponse FromException(Exception exception, string type = "https://tools.ietf.org/html/rfc7231#section-6.6.1")
        {
            return new ErrorResponse(
                type,
                exception.Message,
                exception.InnerException?.Message
            );
        }

        public static ErrorResponse ValidationError(string message, IEnumerable<string> errors)
        {
            return new ErrorResponse(
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                message,
                errors: errors
            );
        }

        public static ErrorResponse NotFound(string message = "The requested resource was not found")
        {
            return new ErrorResponse(
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                message
            );
        }

        public static ErrorResponse Conflict(string message)
        {
            return new ErrorResponse(
                "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                message
            );
        }

        public static ErrorResponse ServiceUnavailable(string message, string? detail = null)
        {
            return new ErrorResponse(
                "https://tools.ietf.org/html/rfc7231#section-6.6.4",
                message,
                detail
            );
        }
    }
}