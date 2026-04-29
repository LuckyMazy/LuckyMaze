using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace LuckyMaze.Infrastructure.Extensions
{
    public static class SemanticKernelExtensions
    {
        public static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
        {
            var apiKey = configuration["OpenRouter:ApiKey"] ?? throw new InvalidOperationException("OpenRouter:ApiKey not configured");

            var defaultModel = configuration["OpenRouter:DefaultModel"] ?? "openai/gpt-5";

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://openrouter.ai/api/v1")
            };
            httpClient.DefaultRequestHeaders.Add("X-Title", "LuckyMaze");

            var builder = services.AddKernel();
            builder.AddOpenAIChatCompletion(defaultModel, apiKey, httpClient: httpClient);

            return services;
        }
    }
}
