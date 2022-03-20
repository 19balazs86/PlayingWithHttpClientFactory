using Microsoft.AspNetCore.Mvc;
using PlayingWithHttpClientFactory.HttpServices;

namespace PlayingWithHttpClientFactory.Controllers
{
  [Route("test")]
  [ApiController]
  public class HttpClientTestController : ControllerBase
  {
    private readonly IUserClient _userClient;

    private readonly ILogger<HttpClientTestController> _logger;

    public HttpClientTestController(IUserClient userClient, ILogger<HttpClientTestController> logger)
    {
      _userClient = userClient;
      _logger     = logger;
    }

    // This method initiate a call to the UserController with the IUserClient.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> Get(CancellationToken ct)
    {
      _logger.LogDebug("HttpClientTestController: Start the call.");

      try
      {
        return Ok(await _userClient.GetUsersAsync(ct));
      }
      catch (ServiceException ex)
      {
        _logger.LogError(ex, "Failed to get users.");

        // Just a dummy response.
        return new ContentResult { StatusCode = 500, Content = $"Message: '{ex.Message}'" };
      }
    }
  }
}