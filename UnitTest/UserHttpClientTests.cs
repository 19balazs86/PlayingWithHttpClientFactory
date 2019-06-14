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
      string[] users = new [] { "User #1", "User #2" };

      // Arrange
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(createResponse(content: users))
        .Verifiable();

      // Act
      IEnumerable<string> response = await SUT.GetUsersAsync();

      // Assert
      Assert.NotNull(response);
      Assert.NotEmpty(response);
      Assert.Equal(users.Length, response.Count());

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
        .ReturnsAsync(createResponse("Just a bad request.", HttpStatusCode.BadRequest));

      // Act + Assert
      await Assert.ThrowsAsync<ServiceException>(() => SUT.GetUsersAsync());
    }

    [Fact]
    public async Task GetUsers_BadContent()
    {
      // Arrange
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(createResponse(content: "For JsonException"));

      // Act + Assert
      await Assert.ThrowsAsync<ServiceException>(() => SUT.GetUsersAsync());
    }

    private static HttpResponseMessage createResponse(
      string content = "",
      HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      return new HttpResponseMessage
      {
        StatusCode = statusCode,
        Content    = new StringContent(content)
      };
    }

    private static HttpResponseMessage createResponse(object content, HttpStatusCode statusCode = HttpStatusCode.OK)
      => createResponse(JsonConvert.SerializeObject(content), statusCode);

    public void Dispose() => _httpClient.Dispose();

    // And so on...
  }
}
