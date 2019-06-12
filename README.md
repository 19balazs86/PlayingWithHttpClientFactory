# Playing with HttpClientFactory
This small application is an example to use the built-in [HttpClientFactory](https://docs.microsoft.com/en-ie/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2 "HttpClientFactory") in ASP.NET Core.

#### What is HttpClientFactory?
- HttpClientFactory provides a central location to configure and create HttpClient instances.
- This concept can be useful to initiate 3rd party services call or even your microservices can call each other.

#### Resources
- Steve Gorgon has [some blog posts about HttpClientFactory](https://www.stevejgordon.co.uk/tag/httpclientfactory "some blog posts about HttpClientFactory") topic.
- Steve Gorgon NDC presentation: [Talk about HTTP in .NET Core](https://www.youtube.com/watch?v=ojDxK_-I-To "Talk about HTTP in .NET Core").
- Blog: [You are (probably still) using HttpClient wrong](https://josefottosson.se/you-are-probably-still-using-httpclient-wrong-and-it-is-destabilizing-your-software/ "You are (probably still) using HttpClient wrong").
- Blog: [How to mock HttpClient in unit tests](https://gingter.org/2018/07/26/how-to-mock-httpclient-in-your-net-c-unit-tests/ "How to mock HttpClient in unit tests").

#### Polly
- I used [Polly](https://github.com/App-vNext/Polly "Polly") as a resilience and transient-fault-handling library, which can helps you to easily write [retry logic](https://docs.microsoft.com/en-ie/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2#use-polly-based-handlers "retry logic"). Other useful information: [Polly and HttpClientFactory](https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory "Polly and HttpClientFactory").
- In the example, I use a timeout policy to cancel a long running call. You can find a solution to use CancellationToken in case, if the client side application cancel the request.

> You can find a similar example in this repository: [Playing with Refit](https://github.com/19balazs86/PlayingWithRefit "Playing with Refit"). Automatic type-safe REST library to initiate http calls.

> In the example, I did not use the [Flurl](https://flurl.io "Flurl") as fluent URL builder and HTTP client library. Worth to check the following article: [Consuming GitHub API (REST) With Flurl](https://code-maze.com/consuming-github-api-rest-with-flurl "Consuming GitHub API (REST) With Flurl").

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
