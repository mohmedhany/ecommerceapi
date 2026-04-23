using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerceApi.Filters;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = new Dictionary<string, string[]>();
            
            foreach (var modelState in context.ModelState)
            {
                var key = modelState.Key;
                var value = modelState.Value;
                var errorMessages = value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>();
                
                if (errorMessages.Length > 0)
                {
                    errors[key] = errorMessages;
                }
            }

            context.Result = new BadRequestObjectResult(new
            {
                statusCode = 400,
                message = "Validation failed",
                errors,
                timestamp = DateTime.UtcNow
            });
        }
    }
}

public class LogActionFilter : IActionFilter
{
    private readonly ILogger<LogActionFilter> _logger;

    public LogActionFilter(ILogger<LogActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var controller = context.Controller.GetType().Name;
        var action = context.ActionDescriptor.DisplayName;
        
        _logger.LogDebug("Executing {Controller}.{Action}", controller, action);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var controller = context.Controller.GetType().Name;
        var action = context.ActionDescriptor.DisplayName;
        var result = context.Result;
        
        _logger.LogDebug("Executed {Controller}.{Action} -> {Result}", controller, action, result?.GetType().Name);
    }
}

public class LogExceptionFilter : IExceptionFilter
{
    private readonly ILogger<LogExceptionFilter> _logger;

    public LogExceptionFilter(ILogger<LogExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        
        _logger.LogError(
            exception,
            "Unhandled exception in {Controller}.{Action}: {Message}",
            context.HttpContext.Request.Path,
            context.ActionDescriptor.DisplayName,
            exception.Message
        );

        context.Result = new ObjectResult(new
        {
            statusCode = 500,
            message = "An internal error occurred",
            timestamp = DateTime.UtcNow
        })
        {
            StatusCode = 500
        };
    }
}