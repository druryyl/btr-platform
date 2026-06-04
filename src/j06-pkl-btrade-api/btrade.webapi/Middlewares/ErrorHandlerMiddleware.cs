using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Nuna.Lib.ActionResultHelper;

namespace btrade.webapi.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, 
        ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            _logger.LogError(error, "--Exception occured: {Message}", error.Message);
            var response = context.Response;
            response.ContentType = "application/json";

            string? status;
            switch (error)
            {
                case ArgumentException:
                case ValidationException:
                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    status = "Bad Request";
                    break;
                case KeyNotFoundException:
                    // not found error
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    status = "Data Not Found";
                    break;
                default:
                    // unhandled error
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    status = "Internal Server Error";
                    break;
            }

            var resultObj = new JSend(response.StatusCode, status, error.Message); 
            var result = JsonSerializer.Serialize(resultObj);
            await response.WriteAsync(result);
        }
    }
}