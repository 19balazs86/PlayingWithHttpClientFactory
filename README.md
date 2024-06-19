# Playing with HttpClientFactory
This small application is an example to use the built-in [HttpClientFactory](https://docs.microsoft.com/en-ie/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.0) in ASP.NET Core.

[Separate branch](https://github.com/19balazs86/PlayingWithHttpClientFactory/tree/netcoreapp2.2) with the .NET Core 2.2 version.

#### What is HttpClientFactory?
- `HttpClientFactory` provides a central location to configure and create `HttpClient` instances.

#### Resources
- [Some blog posts about HttpClientFactory](https://www.stevejgordon.co.uk/tag/httpclientfactory) 📓*Steve Gorgon*
- [Let’s talk about HTTP in .NET Core](https://www.youtube.com/watch?v=Ssii6AwF7Uc) 📽️*45min - Steve Gorgon*
- [You are (probably) using HttpClient wrong](https://josefottosson.se/you-are-probably-still-using-httpclient-wrong-and-it-is-destabilizing-your-software) 📓*Josef Ottosson*
- [Customize the HttpClient logging](https://josef.codes/customize-the-httpclient-logging-dotnet-core) 📓*Josef Ottosson*
- [The right way to use HttpClient](https://www.milanjovanovic.tech/blog/the-right-way-to-use-httpclient-in-dotnet) 📓*Milan's newsletter*
- [How to mock HttpClient in unit tests](https://gingter.org/2018/07/26/how-to-mock-httpclient-in-your-net-c-unit-tests) 📓*Gingter Ale*
- [Delegating handlers to extend HttpClient](https://code-maze.com/aspnetcore-using-delegating-handlers-to-extend-httpclient) 📓*Code-Maze*

#### Unit Test for mocking HttpClient

- Using Moq and Moq.Protected
- Using the [MockHttp](https://github.com/richardszalay/mockhttp) package 👤*Richard Szalay*
- Using the [Pact. NET](https://github.com/pact-foundation/pact-net) package 👤*Pact Foundation*
- Using [WireMock.Net](https://github.com/WireMock-Net/WireMock.Net) 👤*package*
  - [Seamless Integration Testing with WireMock.NET](https://code-maze.com/integration-testing-wiremock-dotnet) 📓*Code-Maze*
  - [Introduction](https://cezarypiatek.github.io/post/mocking-outgoing-http-requests-p1) 📓*Cezary Piątek*
  - [Troubleshooting](https://cezarypiatek.github.io/post/mocking-outgoing-http-requests-p2) - WireMockInspector 📓*Cezary Piątek*
  - [Why my WireMock mocks aren't working?](https://blog.genezini.com/p/why-my-wiremock-mocks-arent-working) 📓*Daniel Genezini*
  


#### Polly
- [Polly docs](https://www.pollydocs.org/) 📓*Official*
- [Build resilient HTTP applications](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience) | [Introduction to resilient app development](https://learn.microsoft.com/en-us/dotnet/core/resilience) 📚*Microsoft-learn*
- [Resilience for a single HttpClient](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines#resilience-with-static-clients) 📚*Microsoft-learn*
- [Building resilient cloud services](https://devblogs.microsoft.com/dotnet/building-resilient-cloud-services-with-dotnet-8) 📓*MS-DevBlog*
- [Apply resilience pipelines when AddHttpClient](https://youtu.be/pgeHRp2Otlc) 📽*32 min - Julio Casal*
- [Resilience Pipelines in .NET 8 with Polly](https://www.milanjovanovic.tech/blog/building-resilient-cloud-applications-with-dotnet) 📓*Milan's newsletter*

> You can find a similar example in this repository: [Playing with Refit](https://github.com/19balazs86/PlayingWithRefit). Automatic type-safe REST library to initiate http calls.

> In the example, I did not use the [Flurl](https://flurl.io) as fluent URL builder and HTTP client library. Worth to check the following article: [Consuming GitHub API (REST) With Flurl](https://code-maze.com/consuming-github-api-rest-with-flurl).

#### Configuration
```csharp
private static void configureResilienceHandler(ResiliencePipelineBuilder<HttpResponseMessage> pipelineBuilder)
{
  var retryOptions = new HttpRetryStrategyOptions
  {
    MaxRetryAttempts = 2,
    Delay            = TimeSpan.FromMilliseconds(500),
    BackoffType      = DelayBackoffType.Constant
  };
  
  pipelineBuilder
    .AddTimeout(TimeSpan.FromSeconds(5)) // Total timeout for the request execution
    .AddRetry(retryOptions)
    .AddTimeout(TimeSpan.FromMilliseconds(500)); // Timeout per each request attempt
}
```
