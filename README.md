# Playing with HttpClientFactory
This small application is an example to use the built-in [HttpClientFactory](https://docs.microsoft.com/en-ie/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.0) in ASP.NET Core.

[Separate branch](https://github.com/19balazs86/PlayingWithHttpClientFactory/tree/netcoreapp2.2) with the .NET Core 2.2 version.

#### What is HttpClientFactory?
- `HttpClientFactory` provides a central location to configure and create `HttpClient` instances.

#### Resources
- [Some blog posts about HttpClientFactory](https://www.stevejgordon.co.uk/tag/httpclientfactory) 📓*Steve Gorgon*
- [Let’s talk about HTTP in .NET Core](https://www.youtube.com/watch?v=Ssii6AwF7Uc) 📽️*45min - Steve Gorgon*
- [You are (probably) using HttpClient wrong](https://josefottosson.se/you-are-probably-still-using-httpclient-wrong-and-it-is-destabilizing-your-software) 📓*Josef Ottosson*
- [How to mock HttpClient in unit tests](https://gingter.org/2018/07/26/how-to-mock-httpclient-in-your-net-c-unit-tests) 📓*Gingter Ale*

#### Unit Test for mocking HttpClient

- Using Moq and Moq.Protected
- Using the [RichardSzalay.MockHttp](https://github.com/richardszalay/mockhttp) 👤*package*
- Using [WireMock.Net](https://github.com/WireMock-Net/WireMock.Net) 👤*package*
  - [WireMock.NET - Introduction](https://cezarypiatek.github.io/post/mocking-outgoing-http-requests-p1) 📓*Cezary Piątek*
  - [Why my WireMock mocks aren't working?](https://blog.genezini.com/p/why-my-wiremock-mocks-arent-working) 📓*Daniel Genezini*


#### Polly
- Using [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault-handling library, which can helps you to easily write [retry logic](https://learn.microsoft.com/en-ie/aspnet/core/fundamentals/http-requests?view=aspnetcore-7.0#use-polly-based-handlers). Other useful information: [Polly and HttpClientFactory](https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory).

- In the example, I use a timeout policy to cancel a long running call. You can find a solution to use `CancellationToken` in case, if the client side application cancel the request.

> You can find a similar example in this repository: [Playing with Refit](https://github.com/19balazs86/PlayingWithRefit). Automatic type-safe REST library to initiate http calls.

> In the example, I did not use the [Flurl](https://flurl.io) as fluent URL builder and HTTP client library. Worth to check the following article: [Consuming GitHub API (REST) With Flurl](https://code-maze.com/consuming-github-api-rest-with-flurl).

#### ConfigureServices in action
```csharp
public void ConfigureServices(IServiceCollection services)
{
  // Add: MessageHandler(s) to the DI container.
  services.AddTransient<TestMessageHandler>();

  // Create: Polly policy
  AsyncRetryPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .Or<TimeoutRejectedException>() // Thrown by Polly's TimeoutPolicy if the inner call gets timeout.
    .WaitAndRetryAsync(_wrc.Retry, _ => TimeSpan.FromMilliseconds(_wrc.Wait));

  AsyncTimeoutPolicy<HttpResponseMessage> timeoutPolicy = Policy
    .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(_wrc.Timeout));

  // Add your service/clients with an interface, helps you to make your business logic testable.
  // --> Add: HttpClient + Polly WaitAndRetry for HTTP 5xx and 408 responses.
  services.AddHttpClient<IUserClient, UserHttpClient>()
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(timeoutPolicy) // The order of adding is imporant!
    // Add: MessageHandler(s).
    .AddHttpMessageHandler<TestMessageHandler>();
}
```
