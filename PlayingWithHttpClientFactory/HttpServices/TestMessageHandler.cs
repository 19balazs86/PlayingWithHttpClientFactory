using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace PlayingWithHttpClientFactory.HttpServices
{
  public class TestMessageHandler : DelegatingHandler
  {
    public TestMessageHandler()
    {
      // The constructor call one time.
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
      // Check request

      HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

      // Do / check something with the response.

      Log.Verbose($"We had a/an {response.StatusCode} status code.");

      return response;
    }
  }
}
