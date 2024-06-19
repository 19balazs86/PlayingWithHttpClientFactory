using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace PlayingWithHttpClientFactory.Controllers;

[Route("[controller]")]
[ApiController]
public sealed class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    private readonly HttpStatusCode[] _httpStatusCodes =
    [
        HttpStatusCode.BadRequest,           // Polly won't retry for this
        HttpStatusCode.NotFound,             // Polly won't retry for this
        HttpStatusCode.UnprocessableContent, // Polly won't retry for this
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK,
        HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK,
    ];

    // This method is called by the HttpClientTestController.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> Get(CancellationToken ct)
    {
        HttpStatusCode selectedStatusCode = _httpStatusCodes[Random.Shared.Next(_httpStatusCodes.Length)];

        _logger.LogDebug("UserController: Selected status code: {selectedStatusCode}", selectedStatusCode);

        // --> Return OK
        if (selectedStatusCode == HttpStatusCode.OK)
            return new string[] { "user 1", "user 2" };

        // --> Delay
        if (selectedStatusCode == HttpStatusCode.RequestTimeout)
        {
            try
            {
                // If your method do not accept token in the argument, you can check it here beforehand.
                ct.ThrowIfCancellationRequested();

                await Task.Delay(5000, ct);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("UserController: The operation was canceled.");

                return NoContent();
            }

            // The timeout policy will end this call earlier, so you won't see this line.
            _logger.LogDebug($"UserController: After the delay.");
        }

        // --> Other returns
        return new ContentResult
        {
            StatusCode = (int)selectedStatusCode,
            Content    = $"Selected status code: {selectedStatusCode}"
        };
    }
}
