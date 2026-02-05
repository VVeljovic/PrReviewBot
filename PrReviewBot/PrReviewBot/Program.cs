using DotNetEnv;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

Env.Load();

var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

if (string.IsNullOrEmpty(apiKey))
{
    Console.Write("Missing key");
    return;
}
string GetDiff()
{
    var psi = new ProcessStartInfo
    {
        FileName = "git",
        Arguments = "diff origin/main...HEAD",
        RedirectStandardOutput = true,
        UseShellExecute = false
    };

    using var p = Process.Start(psi)!;
    return p.StandardOutput.ReadToEnd();
}

var diff = GetDiff();
Console.WriteLine(diff.Length == 0 ? "No diff" : diff);



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