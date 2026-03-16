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

        services.AddHttpClient<FplHttpClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("User-Agent", "FplTool/1.0 (contact@fpltool.app)");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
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
