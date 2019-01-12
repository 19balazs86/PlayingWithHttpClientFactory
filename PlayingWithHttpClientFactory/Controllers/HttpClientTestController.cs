using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlayingWithHttpClientFactory.HttpServices;
using Serilog;

namespace PlayingWithHttpClientFactory.Controllers
{
  [Route("test")]
  [ApiController]
  public class HttpClientTestController : ControllerBase
  {
    private readonly IUserClient _userClient;

    public HttpClientTestController(IUserClient userClient)
    {
      // You may inject your business logic here, not directly the service/client.
      _userClient = userClient;
    }

    // This method initiate a call to the UserController with the IUserClient.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> Get(CancellationToken ct)
    {
      Log.Debug("HttpClientTestController: Start the call.");

      try
      {
        return Ok(await _userClient.GetUsersAsync(ct));
      }
      catch (ServiceException ex)
      {
        Log.Error(ex, "Failed to get users.");

        // Just a dummy response.
        return new ContentResult { StatusCode = 500, Content = $"Message: '{ex.Message}'" };
      }
    }
  }
}