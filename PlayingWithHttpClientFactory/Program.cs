using PlayingWithHttpClientFactory.HttpServices;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;

namespace PlayingWithHttpClientFactory;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;

        // Add services to the container
        {
            services.AddControllers();

            // Add: MessageHandler(s) to the DI container.
            services.AddTransient<TestMessageHandler>();

            // Create: Polly policy
            AsyncRetryPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>() // Thrown by Polly's TimeoutPolicy if the inner call gets timeout.
                .WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(500));

            AsyncTimeoutPolicy<HttpResponseMessage> timeoutPolicy = Policy
                .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(500));

            // Circuit-breaker - Nick Chapsas video: https://youtu.be/3U_TJZU06Ag

            // Add your service/clients with an interface, helps you to make your business logic testable.
            // --> Add: HttpClient + Polly WaitAndRetry for HTTP 5xx and 408 responses.
            services.AddHttpClient<IUserClient, UserHttpClient>()
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy) // The order of adding is imporant!
                .AddHttpMessageHandler<TestMessageHandler>()
                .ConfigureCustomLogging();

            // Configure: Default values for HttpClient
            services.ConfigureHttpClientDefaults(httpClientBuilder => { /* ... */ });
        }

        WebApplication app = builder.Build();

        // Configure the request pipeline
        {
            app.UseHttpsRedirection();

            app.MapControllers();
        }

        app.Run();
    }
}
