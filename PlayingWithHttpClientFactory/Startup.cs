﻿using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlayingWithHttpClientFactory.HttpServices;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace PlayingWithHttpClientFactory
{
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
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

      // Add: MessageHandler(s) to the DI container.
      services.AddTransient<TestMessageHandler>();

      // Create: Polly policy
      Policy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TimeoutRejectedException>() // Thrown by Polly's TimeoutPolicy if the inner call gets timeout.
        .WaitAndRetryAsync(_wrc.Retry, _ => TimeSpan.FromMilliseconds(_wrc.Wait));

      Policy<HttpResponseMessage> timeoutPolicy = Policy
        .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(_wrc.Timeout));

      // Add your service/clients with an interface, helps you to make your business logic testable.
      // --> Add: HttpClient + Polly WaitAndRetry for HTTP 5xx and 408 responses.
      services.AddHttpClient<IUserClient, UserHttpClient>()
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(timeoutPolicy) // The order of adding is imporant!
        // Add: MessageHandler(s).
        .AddHttpMessageHandler<TestMessageHandler>();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      // This is ok for now, but you may create custom middleware or exception filter.
      app.UseDeveloperExceptionPage();

      app.UseMvc();
    }
  }
}
