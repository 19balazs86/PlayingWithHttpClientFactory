using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace PlayingWithHttpClientFactory.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly Random _random = new Random();

    private readonly HttpStatusCode[] _httpStatusCodes = new HttpStatusCode[]
    {
      HttpStatusCode.BadRequest,  // Polly won't retry for this.
      HttpStatusCode.NotFound,    // Polly won't retry for this.
      HttpStatusCode.RequestTimeout,
      HttpStatusCode.RequestTimeout,
      HttpStatusCode.InternalServerError,
      HttpStatusCode.InternalServerError,
      HttpStatusCode.InternalServerError,
      HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK,
      HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK,
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> Get(CancellationToken ct)
    {
      HttpStatusCode selectedStatusCode = _httpStatusCodes[_random.Next(_httpStatusCodes.Length)];

      Log.Debug($"UserController: Selected status code: {selectedStatusCode}");

      // --> Return OK.
      if (selectedStatusCode == HttpStatusCode.OK)
        return new string[] { "user 1", "user 2" };

      // --> Delay.
      if (selectedStatusCode == HttpStatusCode.RequestTimeout)
      {
        try
        {
          // If your method do not accept token as an argument, you can check it here beforehand.
          ct.ThrowIfCancellationRequested();

          await Task.Delay(5000, ct);
        }
        catch (OperationCanceledException)
        {
          Log.Debug("UserController: The operation was canceled.");

          return NoContent();
        }

        // The timeout policy will end this call earlier, so you won't see this line.
        Log.Debug($"UserController: After the delay.");
      }
      
      // --> Other returns.
      return new ContentResult
      {
        StatusCode = (int)selectedStatusCode,
        Content    = $"Selected status code: {selectedStatusCode}"
      };
    }
  }
}
