using Newtonsoft.Json;
using Stripe;
using System.Net;
using TaskAide.Domain.Exceptions;

namespace TaskAide.API.Handlers
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;

        public ExceptionHandler(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
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

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode status;
            string message;

            var exceptionType = exception.GetType();

            if (exceptionType == typeof(NotFoundException))
            {
                status = HttpStatusCode.NotFound;
            }
            else if (exceptionType == typeof(BadRequestException))
            {
                status = HttpStatusCode.BadRequest;
            }
            else if (exceptionType == typeof(StripeException))
            {
                status = HttpStatusCode.BadRequest;
            }
            else if (exceptionType == typeof(UnauthorizedException))
            {
                status = HttpStatusCode.Unauthorized;
            }
            else
            {
                status = HttpStatusCode.InternalServerError;
            }

            message = JsonConvert.SerializeObject(exception.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            return context.Response.WriteAsync(message);
        }
    }
}
