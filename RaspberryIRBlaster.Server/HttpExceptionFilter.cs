using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace RaspberryIRBlaster.Server
{
    public class HttpExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order { get; set; } = int.MaxValue - 10;

        private readonly ILogger _logger;
        public HttpExceptionFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpExceptionFilter>();
        }

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            HandleException(context, context.Exception);
        }

        private void HandleException(ActionExecutedContext context, Exception exception)
        {
            if (exception is AggregateException aggException && aggException.InnerException != null && aggException.InnerExceptions.Count <= 1)
            {
                HandleException(context, aggException.InnerException);
            }
            else if (exception is HttpResponseException httpResponseException)
            {
                context.Result = new ObjectResult(httpResponseException.Message)
                {
                    StatusCode = httpResponseException.StatusCode,
                };
                context.ExceptionHandled = true;
                _logger.LogError($"Exception: {httpResponseException.Message}");
            }
            else if (exception is ArgumentException argumentException)
            {
                context.Result = new ObjectResult(argumentException.Message)
                {
                    StatusCode = 400
                };
                context.ExceptionHandled = true;
                _logger.LogError($"Argument exception: {argumentException.Message}");
            }
        }
    }
}
