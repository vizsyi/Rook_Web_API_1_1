//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
//using System.Threading.Tasks;

namespace Rook01_08.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class CustomExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionHandlingMiddleware> _logger;

        public CustomExceptionHandlingMiddleware(RequestDelegate next
            , ILogger<CustomExceptionHandlingMiddleware> logger)
            //,IDiagnosticContext diagnosticContext)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                if(ex.InnerException is null)
                {
                    _logger.LogError("{ExceptionType} {ExceptionMessage}"
                        , ex.GetType().ToString()
                        , ex.Message);
                }
                else
                {
                    _logger.LogError("{ExceptionType} {ExceptionMessage}"
                        , ex.InnerException.GetType().ToString()
                        , ex.InnerException.Message);
                }

                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsync("Error has occured.");

                //throw; //In case of ExceptionHandling Middleware
            }

        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CustomExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionHandlingMiddleware>();
        }
    }
}
