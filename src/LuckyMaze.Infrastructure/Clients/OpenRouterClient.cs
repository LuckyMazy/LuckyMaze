using LuckyMaze.Infrastructure.Dtos;
using System.Text.Json;

namespace LuckyMaze.Infrastructure.Clients
{
    public class OpenRouterClient(IHttpClientFactory httpClientFactory) : IOpenRouterClient
    {
        public async Task<List<AvailableModelDto>> GetModelsAsync(CancellationToken cancellationToken = default)
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://openrouter.ai/api/v1/models", cancellationToken);
            response.EnsureSuccessStatusCode();

            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

            var models = new List<AvailableModelDto>();

            foreach (var model in json.RootElement.GetProperty("data").EnumerateArray())
            {
                var id = model.GetProperty("id").GetString()!;
                var name = model.GetProperty("name").GetString()!;
                var contextLength = model.GetProperty("context_length").GetInt32();

                var architecture = model.GetProperty("architecture");
                var inputModalities = architecture.GetProperty("input_modalities").EnumerateArray().Select(m => m.GetString()!).ToList();
                var outputModalities = architecture.GetProperty("output_modalities").EnumerateArray().Select(m => m.GetString()!).ToList();

                var pricing = model.GetProperty("pricing");
                var promptPrice = decimal.Parse(pricing.GetProperty("prompt").GetString() ?? "0") * 1_000_000;
                var completionPrice = decimal.Parse(pricing.GetProperty("completion").GetString() ?? "0") * 1_000_000;

                models.Add(new AvailableModelDto(id, name, contextLength, inputModalities, outputModalities, promptPrice, completionPrice));
            }

            return models;
        }
    }
}
