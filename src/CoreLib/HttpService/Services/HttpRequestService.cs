using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreLib.HttpService.Exceptions;
using CoreLib.HttpService.Services.Interfaces;
using CoreLib.HttpService.Services.Models;
using Microsoft.AspNetCore.WebUtilities;
using CoreLib.TraceId.Interfaces;
using CoreLib.TraceId;
using ContentType = CoreLib.HttpService.Services.Models.ContentType;

namespace CoreLib.HttpService.Services;
internal sealed class HttpRequestService : IHttpRequestService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private readonly IHttpConnectionService _httpConnectionService;
    private readonly ITraceWriter _traceWriter;

    public HttpRequestService(IHttpConnectionService httpConnectionService, ITraceWriter traceWriter)
    {
        _httpConnectionService = httpConnectionService;
        _traceWriter = traceWriter;
    }

    public async Task<HttpResponse<TResponse>> SendRequestAsync<TResponse>(
        HttpRequestData requestData,
        HttpConnectionData connectionData = default)
    {
        if (requestData.Uri is null)
        {
            throw new ArgumentNullException(nameof(requestData.Uri), "Request Uri is required");
        }

        var client = _httpConnectionService.CreateHttpClient(connectionData);

        var uri = requestData.QueryParameterList is { Count: > 0 }
            ? new Uri(QueryHelpers.AddQueryString(
                requestData.Uri.ToString(),
                requestData.QueryParameterList.ToDictionary(k => k.Key, v => v.Value)!))
            : requestData.Uri;

        using var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.Method = requestData.Method;
        httpRequestMessage.RequestUri = uri;

        var traceId = _traceWriter.GetValue();
        if (!string.IsNullOrWhiteSpace(traceId))
        {
            httpRequestMessage.Headers.TryAddWithoutValidation(TraceConstants.HeaderName, traceId);
        }

        foreach (var kv in requestData.HeaderDictionary)
        {
            httpRequestMessage.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
        }

        var method = requestData.Method
        if ((method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch ||
             method.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase)) && requestData.Body is not null)
        {
            httpRequestMessage.Content = PrepareContent(requestData.Body, requestData.ContentType);
        }

        using var responseMessage = await _httpConnectionService.SendRequestAsync(httpRequestMessage, client, connectionData.CancellationToken, connectionData.CompletionOption);

        var basePart = new BaseHttpResponse
        {
            StatusCode = responseMessage.StatusCode,
            Headers = responseMessage.Headers,
            ContentHeaders = responseMessage.Content?.Headers
        };

        var responseBytes = responseMessage.Content is null
            ? []
            : await responseMessage.Content.ReadAsByteArrayAsync(connectionData.CancellationToken);

        if (!basePart.IsSuccessStatusCode)
        {
            var bodyString = responseBytes.Length == 0 ? null : Encoding.UTF8.GetString(responseBytes);
            throw new HttpRequestExceptionEx(
                $"HTTP request failed with {(int)basePart.StatusCode} {basePart.StatusCode}",
                (int)basePart.StatusCode,
                bodyString);
        }

        var body = DeserializeBody<TResponse>(responseBytes);

        return new HttpResponse<TResponse>
        {
            StatusCode = basePart.StatusCode,
            Headers = basePart.Headers,
            ContentHeaders = basePart.ContentHeaders,
            Body = body
        };
    }

    private static HttpContent PrepareContent(object body, ContentType contentType)
    {
        switch (contentType)
        {
            case ContentType.ApplicationJson:
                if (body is string jsonText)
                {
                    using var _ = JsonDocument.Parse(jsonText);
                    return new StringContent(jsonText, Encoding.UTF8, MediaTypeNames.Application.Json);
                }
                else
                {
                    var json = JsonSerializer.Serialize(body, JsonOpts);
                    return new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
                }

            case ContentType.XWwwFormUrlEncoded:
                if (body is not IEnumerable<KeyValuePair<string, string>> kv)
                {
                    throw new ArgumentException($"Body for {contentType} must be IEnumerable<KeyValuePair<string,string>>");
                }
                return new FormUrlEncodedContent(kv);

            case ContentType.ApplicationXml:
                return new StringContent(
                    body as string ?? throw new ArgumentException("XML body must be string"),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Xml);

            case ContentType.TextXml:
                return new StringContent(
                    body as string ?? throw new ArgumentException("XML body must be string"),
                    Encoding.UTF8,
                    MediaTypeNames.Text.Xml);

            case ContentType.TextPlain:
            {
                return new StringContent(body?.ToString() ?? string.Empty, Encoding.UTF8, MediaTypeNames.Text.Plain);
            }

            case ContentType.ApplicationJwt:
                var jwt = body as string ?? throw new ArgumentException("JWT body must be string");
                var content = new StringContent(jwt, Encoding.UTF8);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/jwt");
                return content;

            case ContentType.MultipartFormData:
                if (body is not MultipartFormDataContent mfd)
                {
                    throw new ArgumentException("For MultipartFormData pass MultipartFormDataContent instance");
                }
                return mfd;

            case ContentType.Binary:
                if (body is not byte[] bytes)
                {
                    throw new ArgumentException("Binary body must be byte[]");
                }
                return new ByteArrayContent(bytes);

            default:
                throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null);
        }
    }

    private static TResponse? DeserializeBody<TResponse>(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return default;
        }

        if (typeof(TResponse) == typeof(byte[]))
        {
            return (TResponse)(object)bytes;
        }

        var text = Encoding.UTF8.GetString(bytes);
        if (typeof(TResponse) == typeof(string))
        {
            return (TResponse)(object)text;
        }

        try
        {
            var obj = JsonSerializer.Deserialize<TResponse>(text, JsonOpts);
            return obj!;
        }
        catch (Exception ex)
        {
            throw new HttpRequestException("Failed to deserialize");
        }
    }
}