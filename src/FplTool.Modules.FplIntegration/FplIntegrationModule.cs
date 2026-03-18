using FplTool.Modules.FplIntegration.Auth;
using FplTool.Modules.FplIntegration.Caching;
using FplTool.Modules.FplIntegration.Configuration;
using FplTool.Modules.FplIntegration.Contracts;
using FplTool.Modules.FplIntegration.HttpClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FplTool.Modules.FplIntegration;

public static class FplIntegrationModule
{
    public static IServiceCollection AddFplIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FplIntegrationOptions>(configuration.GetSection(FplIntegrationOptions.SectionName));

        var options = configuration.GetSection(FplIntegrationOptions.SectionName).Get<FplIntegrationOptions>()
                      ?? new FplIntegrationOptions();

        services.AddMemoryCache();

        services.AddSingleton<FplAuthService>();
        services.AddTransient<FplAuthHandler>();

        services.AddHttpClient<FplHttpClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9");
            client.DefaultRequestHeaders.Add("Referer", "https://fantasy.premierleague.com/");
            client.DefaultRequestHeaders.Add("Origin", "https://fantasy.premierleague.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<FplAuthHandler>()
        .AddStandardResilienceHandler(resilienceOptions =>
        {
            resilienceOptions.Retry.MaxRetryAttempts = 3;
            resilienceOptions.Retry.Delay = TimeSpan.FromSeconds(2);
            resilienceOptions.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            resilienceOptions.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<FplApiService>();
        services.AddScoped<IFplApiService>(sp =>
            new CachedFplApiService(
                sp.GetRequiredService<FplApiService>(),
                sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
                sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<FplIntegrationOptions>>()
            ));

        return services;
    }
}
