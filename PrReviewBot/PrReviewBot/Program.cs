using DotNetEnv;
using System.Text;
using System.Text.Json;

Env.Load();

var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

if (string.IsNullOrEmpty(apiKey))
{
    Console.Write("Missing key");
    return;
}

var client = new HttpClient();
client.DefaultRequestHeaders.Add("x-api-key", apiKey);
client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");


var payload = new
{
    max_tokens = 1024,
    messages = new[]
    {
        new
        {
            content = "Let mi know how to integrate bitbucket PR pipeline with CLAUDE.",
            role = "user"
        }
    },
    model = "claude-sonnet-4-5-20250929"
};

var response = await client.PostAsync("https://api.anthropic.com/v1/messages", new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));


var json = await response.Content.ReadAsStringAsync();
Console.WriteLine(json);