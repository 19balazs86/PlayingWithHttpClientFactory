using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly.Timeout;

namespace PlayingWithHttpClientFactory.HttpServices
{
  public class UserHttpClient : IUserClient
  {
    private readonly HttpClient _client;

    public UserHttpClient(HttpClient client)
    {
      _client = client;

      // Configure the HttpClient here or outside in the Startup.ConfigureServices.
      _client.BaseAddress = new Uri("http://localhost:5000");

      // You can set also some default settings, like authorization.
      //_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", "token");
    }

    public async Task<IEnumerable<string>> GetUsersAsync(CancellationToken ct = default)
    {
      HttpResponseMessage response = null;

      string contentString;

      try
      {
        // ResponseContentRead waits until both the headers AND content is read.
        // ResponseHeadersRead just reads the headers and then returns. Important to dispose the response!
        // https://www.stevejgordon.co.uk/using-httpcompletionoption-responseheadersread-to-improve-httpclient-performance-dotnet
        response = await _client.GetAsync("User", HttpCompletionOption.ResponseHeadersRead, ct);

        // Throws an exception if the IsSuccessStatusCode property for the HTTP response is false.
        //response.EnsureSuccessStatusCode();

        if (response.IsSuccessStatusCode)
          return await response.Content.ReadAsAsync<IEnumerable<string>>();

        // --> Something wrong: response with 4xx, 5xx status codes.
        contentString = await response.Content.ReadAsStringAsync();
      }
      catch (HttpRequestException ex)
      {
        throw new ServiceException("Could not get the values.", ex);
      }
      catch (JsonException ex)
      {
        throw new ServiceException("Could not convert the response.", ex);
      }
      catch (TimeoutRejectedException ex)
      {
        // If the last try was timeout, Polly throws this own exception.
        throw new ServiceException("Timeout thrown by Polly.", ex);
      }
      catch (OperationCanceledException ex)
      {
        throw new ServiceException("The operation was canceled.", ex);
      }
      finally
      {
        response?.Dispose();
      }

      throw new ServiceException("No Exception, but I could not get the values. " +
        $"StatusCode: {(int)response.StatusCode}, Content: '{contentString}'.");
    }
  }
}
