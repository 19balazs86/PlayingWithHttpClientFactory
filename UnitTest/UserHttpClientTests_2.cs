using PlayingWithHttpClientFactory.HttpServices;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using Xunit;

namespace UnitTest;

public sealed class UserHttpClientTests_2 : IDisposable
{
    private const string _userUrl = "http://localhost:5000/User"; // This is also works"*/User"

    private readonly IUserClient SUT;

    private readonly HttpClient _httpClient;

    private readonly MockHttpMessageHandler _httpMessageHandlerMock;

    public UserHttpClientTests_2()
    {
        _httpMessageHandlerMock = new MockHttpMessageHandler();

        _httpClient = _httpMessageHandlerMock.ToHttpClient();
        //_httpClient = new HttpClient(_httpMessageHandlerMock);

        SUT = new UserHttpClient(_httpClient);
    }

    [Fact]
    public async Task GetUsers_Ok()
    {
        string[] users = ["User #1", "User #2"];

        // Arrange
        _httpMessageHandlerMock
            //.Expect(_userUrl) // Difference between When and Expect: https://github.com/richardszalay/mockhttp
            .When(_userUrl)
            .Respond(HttpStatusCode.OK, JsonContent.Create(users));

        // Act
        IEnumerable<string> response = await SUT.GetUsersAsync();

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response);
        Assert.Equal(users.Length, response.Count());

        _httpMessageHandlerMock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetUsers_BadRequest()
    {
        // Arrange
        _httpMessageHandlerMock
            .When(_userUrl)
            .Respond(HttpStatusCode.BadRequest, MediaTypeNames.Text.Plain, "Just a bad request.");

        // Act + Assert
        await Assert.ThrowsAsync<ServiceException>(() => SUT.GetUsersAsync());
    }

    [Fact]
    public async Task GetUsers_BadContent()
    {
        // Arrange
        _httpMessageHandlerMock
            .When(_userUrl)
            .Respond(HttpStatusCode.OK, JsonContent.Create("For JsonException"));

        // Act + Assert
        await Assert.ThrowsAsync<ServiceException>(() => SUT.GetUsersAsync());
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
