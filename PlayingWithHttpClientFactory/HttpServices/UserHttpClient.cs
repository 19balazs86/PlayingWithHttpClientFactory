using System;
using System.Collections.Generic;
using System.Net.Http;
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

    public async Task<IEnumerable<string>> GetUsersAsync()
    {
      HttpResponseMessage response;

      try
      {
        // --> Get.
        response = await _client.GetAsync("User");

        // Throws an exception if the IsSuccessStatusCode property for the HTTP response is false.
        //response.EnsureSuccessStatusCode();

        // --> Check and return if so.
        if (response.IsSuccessStatusCode)
          return await response.Content.ReadAsAsync<IEnumerable<string>>();
      }
      catch (HttpRequestException ex)
      {
        throw new ServiceException("Sorry, I could not get the values.", ex);
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

      // --> Something wrong.
      string contentString = await response.Content.ReadAsStringAsync();

      throw new ServiceException("Sorry, I could not get the values. " +
        $"StatusCode: {(int)response.StatusCode}, Content: '{contentString}'.");
    }
  }
}
