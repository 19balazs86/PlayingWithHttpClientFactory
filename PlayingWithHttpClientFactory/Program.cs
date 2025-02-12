﻿using Microsoft.Extensions.Http.Resilience;
using PlayingWithHttpClientFactory.HttpServices;
using Polly;

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

            services.AddTransient<TestMessageHandler>();

            // You can use this predefined pipeline via DI. Example: Milan's newsletter in the README
            // services.AddResiliencePipeline<string, GitHubUser?>("gh-fallback", builder => { });

            // --> Add: HttpClient with resilience
            services.AddHttpClient<IUserClient, UserHttpClient>()
                .AddHttpMessageHandler<TestMessageHandler>()
                .ConfigureCustomLogging()
                // .AddTransientHttpErrorPolicy(p => p.RetryAsync(3)) // Non-Resilience version: Install-Package Microsoft.Extensions.Http.Polly
                // .AddStandardResilienceHandler()
                .AddResilienceHandler("user-pipeline", configureResilienceHandler);

            // Configure: Default values for HttpClient
            services.ConfigureHttpClientDefaults(httpClientBuilder => { /* ... */ });
        }

        WebApplication app = builder.Build();

        // Configure the request pipeline
        {
            app.MapControllers();
        }

        app.Run();
    }

    private static void configureResilienceHandler(ResiliencePipelineBuilder<HttpResponseMessage> pipelineBuilder)
    {
        // Circuit-breaker - Nick Chapsas video for older version of Polly: https://youtu.be/3U_TJZU06Ag

        // --> Define option: Retry
        var retryOptions = new HttpRetryStrategyOptions
        {
            // ShouldHandle = ... this is set by default in HttpRetryStrategyOptions
            MaxRetryAttempts = 2,
            Delay            = TimeSpan.FromMilliseconds(500),
            MaxDelay         = TimeSpan.FromMilliseconds(500),
            BackoffType      = DelayBackoffType.Constant,
            OnRetry = arg =>
            {
                // This is not necessary because Polly does logging by default, which you can manage in appsettings.json
                //Console.WriteLine("AttemptNumber: {0}, Duration: {1}, Result.StatusCode: {2}, Exception: '{3}'", arg.AttemptNumber, arg.Duration, arg.Outcome.Result?.StatusCode, arg.Outcome.Exception?.Message);

                return ValueTask.CompletedTask;
            }
        };

        // --> Configure: Pipeline
        pipelineBuilder
            .AddTimeout(TimeSpan.FromSeconds(5)) // Total timeout for the request execution
            .AddRetry(retryOptions)
            .AddTimeout(TimeSpan.FromMilliseconds(500)); // Timeout per each request attempt
    }
}
