using Polly.Timeout;
using System.Text.Json;

namespace PlayingWithHttpClientFactory.HttpServices;

public sealed class UserHttpClient : IUserClient
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

        // Error handling (Josef): https://josef.codes/httpclient-error-handling-a-test-driven-approach
        try
        {
            // ResponseContentRead waits until both the headers AND content is read.
            // ResponseHeadersRead just reads the headers and then returns. Important to dispose the response!
            // Link #1: https://www.stevejgordon.co.uk/using-httpcompletionoption-responseheadersread-to-improve-httpclient-performance-dotnet
            // Link #2: https://steven-giesel.com/blogPost/e2c3bcba-4f81-42b0-9b25-060da5e819fa
            response = await _client.GetAsync("User", HttpCompletionOption.ResponseHeadersRead, ct);

            // Throws an exception if the IsSuccessStatusCode property for the HTTP response is false.
            //response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<IEnumerable<string>>(ct);

            // --> Something wrong: response with 4xx, 5xx status codes.
            contentString = await response.Content.ReadAsStringAsync(ct);
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException("Could not get the values.", ex);
        }
        catch (JsonException ex)
        {
            throw new ServiceException("The JSON is invalid.", ex);
        }
        catch (TimeoutRejectedException ex) // ExecutionRejectedException can be use to catch Polly's exceptions: https://www.pollydocs.org/api/Polly.ExecutionRejectedException.html
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
