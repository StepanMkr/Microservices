using CoreLib.TraceId.Interfaces;

namespace CoreLib.TraceId;

public class TraceIdHttpMessageHandler : DelegatingHandler
{
    private readonly ITraceWriter _traceWriter;

    public TraceIdHttpMessageHandler(ITraceWriter traceWriter)
    {
        _traceWriter = traceWriter;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var current = _traceWriter.GetValue();
        if (!string.IsNullOrWhiteSpace(current))
        {
            if (!request.Headers.Contains("X-Trace-Id"))
            {
                request.Headers.Add("X-Trace-Id", current);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}