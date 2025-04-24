using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace CleaningMyName.Api.Extensions;

public static class PollyExtensions
{
    public static IServiceCollection AddPolly(this IServiceCollection services)
    {
        // Add HttpClient with Polly policies
        services.AddHttpClient("DefaultClient")
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
            
        return services;
    }
    
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
    
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
