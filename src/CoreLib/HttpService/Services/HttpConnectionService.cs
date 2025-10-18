using CoreLib.HttpService.Exceptions;
using CoreLib.HttpService.Services.Interfaces;
using CoreLib.HttpService.Services.Models;

namespace CoreLib.HttpService.Services;
internal sealed class HttpConnectionService : IHttpConnectionService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpConnectionService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public HttpClient CreateHttpClient(HttpConnectionData data)
    {
        var httpClient = string.IsNullOrWhiteSpace(data.ClientName)
            ? _httpClientFactory.CreateClient()
            : _httpClientFactory.CreateClient(data.ClientName);

        if (data.Timeout is not null)
            httpClient.Timeout = data.Timeout.Value;

        return httpClient;
    }

    public async Task<HttpResponseMessage> SendRequestAsync(
        HttpRequestMessage httpRequestMessage,
        HttpClient httpClient,
        CancellationToken cancellationToken,
        HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        try
        {
            return await httpClient.SendAsync(httpRequestMessage, httpCompletionOption, cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new HttpRequestException("HTTP request error", ex);
        }
    }
}