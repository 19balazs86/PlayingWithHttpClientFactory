using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using PlayingWithHttpClientFactory.HttpServices;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Xunit;

namespace UnitTest;

public sealed class UserHttpClientTests : IDisposable
{
    private readonly IUserClient SUT;

    private readonly HttpClient _httpClient;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public UserHttpClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        SUT = new UserHttpClient(_httpClient);
    }

    [Fact]
    public async Task GetUsers_Ok()
    {
        string[] users = new[] { "User #1", "User #2" };

        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(createResponse(content: users))
            .Verifiable();

        // The MemoryStream (in createResponse) will be closed after the GetUsersAsync due to response?.Dispose().

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

    public void Dispose() => _httpClient.Dispose();

    #region Helper methods

    private static HttpResponseMessage createResponse(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        => createResponse(new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json), statusCode);

    private static HttpResponseMessage createResponse(object content, HttpStatusCode statusCode = HttpStatusCode.OK)
        => createResponse(createHttpStreamContent(content), statusCode);

    private static HttpResponseMessage createResponse(
        HttpContent content,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage
        {
            Content    = content,
            StatusCode = statusCode
        };
    }

    private static HttpContent createHttpStreamContent(object content)
    {
        var memoryStream = new MemoryStream();

        serializeJsonIntoStream(content, memoryStream);

        memoryStream.Seek(0, SeekOrigin.Begin);

        HttpContent httpContent = new StreamContent(memoryStream);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

        return httpContent;
    }

    private static void serializeJsonIntoStream(object value, Stream stream)
    {
        using var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true);
        using var jsonTextWriter = new JsonTextWriter(streamWriter);

        new JsonSerializer().Serialize(jsonTextWriter, value);

        jsonTextWriter.Flush();
    }

    // Efficient post calls with HttpClient and JSON.NET
    // https://johnthiriet.com/efficient-post-calls

    #endregion
}
