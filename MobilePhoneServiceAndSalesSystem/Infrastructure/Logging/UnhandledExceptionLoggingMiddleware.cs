namespace MobilePhoneServiceAndSalesSystem.Infrastructure.Logging
{
    public class UnhandledExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UnhandledExceptionLoggingMiddleware> _logger;

        public UnhandledExceptionLoggingMiddleware(
            RequestDelegate next,
            ILogger<UnhandledExceptionLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception for {Method} {Path} User={User}",
                    context.Request.Method,
                    context.Request.Path,
                    context.User.Identity?.Name ?? "Anonymous");

                throw;
            }
        }
    }
}
