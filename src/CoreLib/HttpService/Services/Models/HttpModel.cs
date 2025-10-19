using System.Net;
using System.Net.Http.Headers;

namespace CoreLib.HttpService.Services.Models;
public enum ContentType
{
    Unknown = 0,
    ApplicationJson = 1,
    XWwwFormUrlEncoded = 2,
    Binary = 3,
    ApplicationXml = 4,
    MultipartFormData = 5,
    TextXml = 6,
    TextPlain = 7,
    ApplicationJwt = 8
}

public readonly record struct HttpConnectionData()
{
    public TimeSpan? Timeout { get; init; }

    public CancellationToken CancellationToken { get; init; }

    public string? ClientName { get; init; }

    public HttpCompletionOption CompletionOption { get; init; } = HttpCompletionOption.ResponseContentRead;
}

public record HttpRequestData
{
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    public Uri? Uri { get; set; }

    public object? Body { get; set; }

    public ContentType ContentType { get; set; } = ContentType.ApplicationJson;

    public IDictionary<string, string> HeaderDictionary { get; set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public ICollection<KeyValuePair<string, string>> QueryParameterList { get; set; } =
        new List<KeyValuePair<string, string>>();
}

public record BaseHttpResponse
{
    public HttpStatusCode StatusCode { get; init; }

    public HttpResponseHeaders Headers { get; init; } = null!;

    public HttpContentHeaders? ContentHeaders { get; init; }

    public bool IsSuccessStatusCode => (int)StatusCode is >= 200 and <= 299;
}

public sealed record HttpResponse<TResponse> : BaseHttpResponse
{
    public TResponse? Body { get; init; }
}