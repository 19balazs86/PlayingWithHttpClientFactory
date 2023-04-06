using PlayingWithHttpClientFactory.HttpServices;
using System.Net.Mime;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace UnitTest;

public sealed class UserHttpClientTests_3 : IDisposable
{
    private readonly IUserClient SUT;

    private readonly HttpClient _httpClient;

    private readonly WireMockServer _wireMockServer;

    private readonly IRequestBuilder _userRequest;

    public UserHttpClientTests_3()
    {
        _wireMockServer = WireMockServer.StartWithAdminInterface(port: 5000, ssl: false);

        _httpClient = _wireMockServer.CreateClient();

        SUT = new UserHttpClient(_httpClient);

        _userRequest = Request.Create().WithPath("/User").UsingGet();
    }

    [Fact]
    public async Task GetUsers_Ok()
    {
        // Arrange
        string[] users = new[] { "User #1", "User #2" };

        _wireMockServer
            //.Given(Request.Create().WithPath("/User").UsingGet())
            .Given(_userRequest)
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                .WithBodyAsJson(users));

        // Act
        IEnumerable<string> response = await SUT.GetUsersAsync();

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response);
        Assert.Equal(users.Length, response.Count());
    }

    [Fact]
    public async Task GetUsers_BadRequest()
    {
        // Arrange
        _wireMockServer
            .Given(_userRequest)
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithHeader("Content-Type", MediaTypeNames.Text.Plain)
                .WithBody("Just a bad request"));

        // Act + Assert
        await Assert.ThrowsAsync<ServiceException>(() => SUT.GetUsersAsync());
    }

    [Fact]
    public async Task GetUsers_BadContent()
    {
        // Arrange
        _wireMockServer
            .Given(_userRequest)
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                .WithBody("For JsonException"));

        // Act + Assert
        await Assert.ThrowsAsync<ServiceException>(() => SUT.GetUsersAsync());
    }

    public void Dispose()
    {
        _wireMockServer.Dispose();

        _httpClient.Dispose();
    }
}
