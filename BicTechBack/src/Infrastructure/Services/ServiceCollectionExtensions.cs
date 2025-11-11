using Infrastructure.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Infrastructure.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStripeHttpClient(this IServiceCollection services)
        {
            services.AddHttpClient("Stripe", (sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<StripeOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("BicTechBack/1.0");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", opts.SecretKey);
            })
            .AddStandardResilienceHandler();

            return services;
        }
    }
}