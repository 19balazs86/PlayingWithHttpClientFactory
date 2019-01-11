using System.Collections.Generic;
using System.Net;
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
      // Here, you may inject your business logic and not directly the service/client.
      _userClient = userClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> Get()
    {
      try
      {
        return Ok(await _userClient.GetUsersAsync());
      }
      catch (ServiceException ex)
      {
        Log.Error(ex, "Failed to get users.");

        // Just a dummy response.
        return new ContentResult
        {
          StatusCode = (int) HttpStatusCode.InternalServerError,
          Content    = $"Message: '{ex.Message}'"
        };
      }
    }
  }
}