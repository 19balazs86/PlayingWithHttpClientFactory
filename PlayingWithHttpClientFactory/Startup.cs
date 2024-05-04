using PlayingWithHttpClientFactory.HttpServices;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;

namespace PlayingWithHttpClientFactory;

public class Startup
{
    private readonly WaitAndRetryConfig _wrc = new WaitAndRetryConfig();

    public Startup(IConfiguration configuration)
    {
        // --> Prepare configurations.
        configuration.GetSection("WaitAndRetry").Bind(_wrc);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // Add: MessageHandler(s) to the DI container.
        services.AddTransient<TestMessageHandler>();

        // Create: Polly policy
        AsyncRetryPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>() // Thrown by Polly's TimeoutPolicy if the inner call gets timeout.
            .WaitAndRetryAsync(_wrc.Retry, _ => TimeSpan.FromMilliseconds(_wrc.Wait));

        AsyncTimeoutPolicy<HttpResponseMessage> timeoutPolicy = Policy
            .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(_wrc.Timeout));

        // Circuit-breaker - Nick Chapsas video: https://youtu.be/3U_TJZU06Ag

        // Add your service/clients with an interface, helps you to make your business logic testable.
        // --> Add: HttpClient + Polly WaitAndRetry for HTTP 5xx and 408 responses.
        services.AddHttpClient<IUserClient, UserHttpClient>()
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(timeoutPolicy) // The order of adding is imporant!
            .AddHttpMessageHandler<TestMessageHandler>()
            .ConfigureCustomLogging();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // This is ok for now, but you may create custom middleware or exception filter.
        app.UseDeveloperExceptionPage();

        app.UseRouting();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
