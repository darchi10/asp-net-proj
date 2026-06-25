using System.Text;
using System.Text.Json;

namespace MobilePhoneServiceAndSalesSystem.Infrastructure.AI;

public class GroqAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GroqAiService> _logger;
    private const string Model = "llama-3.1-8b-instant";

    public GroqAiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GroqAiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = configuration["Groq:ApiKey"] ?? throw new InvalidOperationException("Groq API key not configured");
        _logger = logger;
    }

    public async Task<T?> ParseToEntityAsync<T>(string userInput, string systemPrompt) where T : class
    {
        try
        {
            var messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userInput }
            };

            var request = new
            {
                model = Model,
                messages,
                temperature = 0.3,
                max_tokens = 500
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GroqResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var aiResponse = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            if (string.IsNullOrWhiteSpace(aiResponse)) return null;

            // Extract JSON from response (handle markdown code blocks)
            var jsonStart = aiResponse.IndexOf('{');
            var jsonEnd = aiResponse.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonString = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                return JsonSerializer.Deserialize<T>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Groq AI: {Message}", ex.Message);
            return null;
        }
    }

    private class GroqResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }
}
