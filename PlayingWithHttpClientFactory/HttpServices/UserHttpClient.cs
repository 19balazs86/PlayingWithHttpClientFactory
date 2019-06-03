using System;
using System.Collections.Generic;
using System.IO;
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
    private readonly JsonSerializer _jsonSerializer;

    public UserHttpClient(HttpClient client, JsonSerializer jsonSerializer)
    {
      _client = client;

      // Configure the HttpClient here or outside in the Startup.ConfigureServices.
      _client.BaseAddress = new Uri("http://localhost:5000");

      // You can set also some default settings, like authorization.
      //_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", "token");

      _jsonSerializer = jsonSerializer;
    }

    public async Task<IEnumerable<string>> GetUsersAsync(CancellationToken ct)
    {
      HttpResponseMessage response;

      try
      {
        // ResponseContentRead waits until both the headers AND content is read.
        // ResponseHeadersRead just reads the headers and then returns.
        response = await _client.GetAsync("User", HttpCompletionOption.ResponseHeadersRead, ct);

        // Throws an exception if the IsSuccessStatusCode property for the HTTP response is false.
        //response.EnsureSuccessStatusCode();

        if (response.IsSuccessStatusCode)
        {
          // If you have a large object in the response. It is better than ReadAsAsync.
          using (Stream responseStream = await response.Content.ReadAsStreamAsync())
          using (var streamReader      = new StreamReader(responseStream))
          using (var jsonTextReader    = new JsonTextReader(streamReader))
            return _jsonSerializer.Deserialize<IEnumerable<string>>(jsonTextReader);
        }
      }
      catch (HttpRequestException ex)
      {
        throw new ServiceException("Could not get the values.", ex);
      }
      catch (JsonReaderException ex)
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

      // --> Something wrong: response with 4xx, 5xx status codes.
      string contentString = await response.Content.ReadAsStringAsync();

      throw new ServiceException("No Exception, but I could not get the values. " +
        $"StatusCode: {(int)response.StatusCode}, Content: '{contentString}'.");
    }
  }
}
