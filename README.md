# Playing with HttpClientFactory

This small application is an example to use the built-in [HttpClientFactory](https://docs.microsoft.com/en-ie/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2 "HttpClientFactory") in ASP.NET Core.
HttpClientFactory provides a central location to configure and create HttpClient instances.

This concept can be useful to initiate 3rd party services call or even your microservices can call each other (be careful with the dependency).

Use [Polly](https://github.com/App-vNext/Polly "Polly") as a resilience and transient-fault-handling library, which can helps you to easily write [retry logic](https://docs.microsoft.com/en-ie/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2#use-polly-based-handlers "retry logic").
Other useful information: [Polly and HttpClientFactory](https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory "Polly and HttpClientFactory").

In the example, I use a timeout policy to cancel a long running call. You can find a solution to use CancellationToken in case, if the client side application cancel the request.

According to the Microsoft description, seems easy, but there are numerous of things, which you should handle.

In the example, I did not use the [Flurl](https://flurl.io "Flurl") as fluent URL builder and HTTP client library. Worth to check the following article: [Consuming GitHub API (REST) With Flurl.](https://code-maze.com/consuming-github-api-rest-with-flurl "Consuming GitHub API (REST) With Flurl") 

You can find some inline comments in the code, like this:
```csharp
public void ConfigureServices(IServiceCollection services)
{
  ...
  // Add your service with an interface, helps you to make your business logic testable.
  services.AddHttpClient<IUserClient, UserHttpClient>()
  ...
}
```
