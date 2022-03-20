namespace PlayingWithHttpClientFactory.HttpServices
{
  public class TestMessageHandler : DelegatingHandler
  {
    private readonly ILogger<TestMessageHandler> _logger;

    public TestMessageHandler(ILogger<TestMessageHandler> logger)
    {
      // The constructor call one time.

      _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
      // Check request

      HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

      // Do / check something with the response.

      _logger.LogInformation($"We had a/an {response.StatusCode} status code.");

      return response;
    }
  }
}
