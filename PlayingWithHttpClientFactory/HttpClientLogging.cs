using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Logging;

namespace PlayingWithHttpClientFactory;

public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder ConfigureCustomLogging(this IHttpClientBuilder builder)
    {
        builder.Services.TryAddScoped<HttpClientLogging>();

        builder.RemoveAllLoggers();

        builder.AddLogger<HttpClientLogging>(wrapHandlersPipeline: true);

        return builder;
    }
}

// Customize the HttpClient logging: https://josef.codes/customize-the-httpclient-logging-dotnet-core
public sealed class HttpClientLogging : IHttpClientLogger
{
    private readonly ILogger<HttpClientLogging> _logger;

    public HttpClientLogging(ILogger<HttpClientLogging> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogRequestFailed(object context, HttpRequestMessage request, HttpResponseMessage response, Exception exception, TimeSpan elapsed)
    {
        _logger.LogError(
            exception,
            "Request towards '{Request.Host}{Request.Path}' failed after {Response.ElapsedMilliseconds} ms",
            request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped),
            request.RequestUri!.PathAndQuery,
            elapsed.TotalMilliseconds.ToString("F1"));
    }

    public object LogRequestStart(HttpRequestMessage request)
    {
        _logger.LogInformation(
            "Sending '{Request.Method}' to '{Request.Host}{Request.Path}'",
            request.Method,
            request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped),
            request.RequestUri!.PathAndQuery);

        return null;
    }

    public void LogRequestStop(object context, HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
    {
        _logger.LogInformation(
            "Received '{Response.StatusCodeInt} {Response.StatusCodeString}' after {Response.ElapsedMilliseconds} ms",
            (int)response.StatusCode,
            response.StatusCode,
            elapsed.TotalMilliseconds.ToString("F1"));
    }
}
