using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Backend.services.Middleware
{
    public class ChatLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ChatLoggingMiddleware> _logger;

        public ChatLoggingMiddleware(RequestDelegate next, ILogger<ChatLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //* Check if this is a chat-related request
            if (context.Request.Path.StartsWithSegments("/api/ai"))
            {
                //* Log the request
                var requestPath = context.Request.Path;
                var requestMethod = context.Request.Method;
                var requestTime = DateTime.Now;

                _logger.LogInformation($"Chat request: {requestMethod} {requestPath} at {requestTime}");

                //* For POST requests, log the request body (but not for production)
                if (requestMethod == "POST" && context.Request.ContentLength > 0)
                {
                    context.Request.EnableBuffering();

                    using (var reader = new StreamReader(
                        context.Request.Body,
                        encoding: Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: false,
                        leaveOpen: true))
                    {
                        var requestBody = await reader.ReadToEndAsync();
                        _logger.LogDebug($"Request body: {requestBody}");

                        //* Reset the request body position
                        context.Request.Body.Position = 0;
                    }
                }

                //* Capture the original response body
                var originalBodyStream = context.Response.Body;

                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    //* Continue processing
                    await _next(context);

                    //* Log the response
                    var statusCode = context.Response.StatusCode;
                    var responseTime = DateTime.Now;
                    var duration = responseTime - requestTime;

                    _logger.LogInformation($"Chat response: {statusCode} for {requestMethod} {requestPath} in {duration.TotalMilliseconds}ms");

                    //* Copy the response to the original stream
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            else
            {
                //* Not a chat request, just continue
                await _next(context);
            }
        }
    }
}
