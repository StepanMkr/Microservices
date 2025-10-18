using CoreLib.HttpService.Services.Models;

namespace CoreLib.HttpService.Services.Interfaces;

/// <summary>
/// Функционал для создания HttpClient и отправки запросов
/// </summary>
internal interface IHttpConnectionService
{
    /// <summary>
    /// Создать настроенный HttpClient с учётом таймаута и имени клиента
    /// </summary>
    HttpClient CreateHttpClient(HttpConnectionData httpConnectionData);

    /// <summary>
    /// Отправить HTTP-запрос через переданный HttpClient
    /// </summary>
    Task<HttpResponseMessage> SendRequestAsync(
        HttpRequestMessage httpRequestMessage,
        HttpClient httpClient,
        CancellationToken cancellationToken,
        HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead);
}