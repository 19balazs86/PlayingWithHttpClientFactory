using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using PlayingWithHttpClientFactory.HttpServices;
using Xunit;

namespace UnitTest
{
  public class UserHttpClientTests : IDisposable
  {
    private readonly IUserClient SUT;

    private readonly HttpClient _httpClient;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public UserHttpClientTests()
    {
      _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

      _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

      SUT = new UserHttpClient(_httpClient, new JsonSerializer());
    }

    [Fact]
    public async Task GetUsers_Ok()
    {
      IEnumerable<string> users = new [] { "User #1", "User #2" };

      // Arrange
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(createResponse(content: users))
        .Verifiable();

      // Act
      IEnumerable<string> response = await SUT.GetUsersAsync(CancellationToken.None);

      // Assert
      Assert.NotNull(response);
      Assert.NotEmpty(response);
      Assert.Equal(users.Count(), response.Count());

      _httpMessageHandlerMock
        .Protected()
        .Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetUsers_BadRequest()
    {
      // Arrange
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(createResponse(HttpStatusCode.BadRequest, "Just a bad request."));

      // Act + Assert
      await Assert.ThrowsAsync<ServiceException>(() => SUT.GetUsersAsync(CancellationToken.None));
    }

    private static HttpResponseMessage createResponse(
      HttpStatusCode statusCode = HttpStatusCode.OK,
      object content            = null)
    {
      return new HttpResponseMessage
      {
        StatusCode = statusCode,
        Content    = new StringContent(JsonConvert.SerializeObject(content))
      };
    }

    public void Dispose() => _httpClient.Dispose();

    // And so on...
  }
}
