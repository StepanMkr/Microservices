using CoreLib.TraceId.Interfaces;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace CoreLib.TraceId
{
    public class TraceIdMiddleware
    {
        private readonly RequestDelegate _next;

        public TraceIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITraceReader traceReader, ITraceWriter traceWriter)
        {
            var traceId = context.Request.Headers.TryGetValue("X-Trace-Id", out var values)
                ? values.ToString()
                : null;

            traceReader.WriteValue(traceId ?? string.Empty);

            using (LogContext.PushProperty("TraceId", traceWriter.GetValue()))
            {
                context.Response.OnStarting(() =>
                {
                    var current = traceWriter.GetValue();
                    if (!string.IsNullOrWhiteSpace(current))
                    {
                        context.Response.Headers["X-Trace-Id"] = current;
                    }

                    return Task.CompletedTask;
                });

                await _next(context);
            }
        }
    }
}