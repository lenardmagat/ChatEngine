using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ChatSystem.DTOs;
using System.Net;
namespace ChatSystem.GlobalException
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IHostEnvironment _env;
        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "An unhandled exception occurred in the application.");
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred on our server.";
            if (context.Exception is KeyNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound; 
                message = context.Exception.Message;
            }
            else if (context.Exception is InvalidOperationException || context.Exception is ArgumentException)
            {
                statusCode = HttpStatusCode.BadRequest; 
                message = context.Exception.Message;
            }
            var errorResponse = new ErrorResponse
            {
                StatusCode = (int)statusCode,
                Message = message,
                Details = _env.IsDevelopment() ? context.Exception.StackTrace : null 
            };
            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = (int)statusCode
            };
            context.ExceptionHandled = true;
        }
    }
}