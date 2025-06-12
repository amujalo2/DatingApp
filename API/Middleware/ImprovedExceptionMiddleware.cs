using System;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using System.Security.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using API.Errors;

namespace API.Middleware;

public class ImprovedExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ImprovedExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;
    private readonly Dictionary<Type, HttpStatusCode> _exceptionMappings;

    public ImprovedExceptionMiddleware(RequestDelegate next, ILogger<ImprovedExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
        
        _exceptionMappings = new Dictionary<Type, HttpStatusCode>
        {
            // System exceptions
            { typeof(UnauthorizedAccessException), HttpStatusCode.Unauthorized },
            { typeof(AuthenticationException), HttpStatusCode.Unauthorized },
            { typeof(KeyNotFoundException), HttpStatusCode.NotFound },
            { typeof(ArgumentException), HttpStatusCode.BadRequest },
            { typeof(InvalidOperationException), HttpStatusCode.BadRequest },
            { typeof(FormatException), HttpStatusCode.BadRequest },
            { typeof(TimeoutException), HttpStatusCode.RequestTimeout },
            
            // Customized exceptions
            { typeof(NotFoundException), HttpStatusCode.NotFound },
            { typeof(BadRequestException), HttpStatusCode.BadRequest },
            { typeof(UnauthorizedException), HttpStatusCode.Unauthorized },
            { typeof(ForbiddenException), HttpStatusCode.Forbidden },
            { typeof(ConflictException), HttpStatusCode.Conflict }
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
            
            // Catch 404 for endpoints that do not exist
            if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                var response = new ApiException(404, "Endpoint nije pronađen", "URL putanja nije validna");
                await WriteResponse(context, response);
            }
            // Catch 401 for unauthorized accesses
            else if (context.Response.StatusCode == 401 && !context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                var response = new ApiException(401, "Niste autorizovani", "Potrebna je autentifikacija");
                Console.WriteLine("TEST\n");
                await WriteResponse(context, response);
            }
            // Catch 403 for forbidden accesses
            else if (context.Response.StatusCode == 403 && !context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                var response = new ApiException(403, "Pristup zabranjen", "Nemate dozvole za pristup ovom resursu");
                await WriteResponse(context, response);
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        string errorMessage = exception.Message;
        string? details = null;

        // Check if the exception corresponds to one of the predefined types.
        foreach (var mapping in _exceptionMappings)
        {
            if (exception.GetType() == mapping.Key || exception.GetType().IsSubclassOf(mapping.Key))
            {
                statusCode = mapping.Value;
                break;
            }
        }

        // Log different information depending on the severity of the error.
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, $"KRITIČNA GREŠKA: {errorMessage}");
            details = _env.IsDevelopment() ? exception.StackTrace : "Interna greška servera";
        }
        else
        {
            string statusName = statusCode.ToString();
            _logger.LogWarning(exception, $"{(int)statusCode} {statusName}: {errorMessage}");
            details = _env.IsDevelopment() ? exception.StackTrace : null;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiException(context.Response.StatusCode, errorMessage, details);
        await WriteResponse(context, response);
    }

    private async Task WriteResponse(HttpContext context, ApiException response)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}