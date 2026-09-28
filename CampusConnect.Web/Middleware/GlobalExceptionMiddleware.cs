using System.Net;
using System.Text.Json;

namespace CampusConnect.Web.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Si es una petición API, responder en JSON
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    success = false,
                    message = "Ocurrió un error interno en el servidor.",
                    detail = exception.Message
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            else
            {
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}
