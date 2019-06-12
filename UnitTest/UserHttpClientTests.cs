using System.Collections.Generic;
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
  public class UserHttpClientTests
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
      // Arrange
      HttpResponseMessage response = new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content    = new StringContent("['User #1', 'User #2']")
      };

      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(response)
        .Verifiable();

      // Act
      IEnumerable<string> users = await SUT.GetUsersAsync(CancellationToken.None);

      // Assert
      Assert.NotNull(users);
      Assert.NotEmpty(users);

      _httpMessageHandlerMock
        .Protected()
        .Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetUsers_BadRequest()
    {
      // Arrange
      HttpResponseMessage response = new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.BadRequest,
        Content    = new StringContent("Just a bad request.")
      };

      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(response);

      // Act + Assert
      await Assert.ThrowsAsync<ServiceException>(() => SUT.GetUsersAsync(CancellationToken.None));
    }

    // And so on...
  }
}
