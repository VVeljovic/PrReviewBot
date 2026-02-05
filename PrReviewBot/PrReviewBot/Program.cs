using DotNetEnv;
using PrReviewBot;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

Env.Load();

var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

if (string.IsNullOrEmpty(apiKey))
{
    Console.Write("key not found");
    return;
}
string GetDiff()
{
    var psi = new ProcessStartInfo
    {
        FileName = "git",
        Arguments = "diff origin/main...HEAD --",
        RedirectStandardOutput = true,
        UseShellExecute = false
    };

    using var p = Process.Start(psi)!;
    return p.StandardOutput.ReadToEnd();
}

var diff = GetDiff();
var prompt = diff.Length == 0
    ? "The PR has no changes. Provide a brief confirmation message."
    : $@"
You are an expert code reviewer. Analyze the following git diff and provide:
1. A summary of what changed.
2. Any potential bugs or issues.
3. Suggestions for improving code quality.
4. Recommendations for tests or documentation updates.

Provide your review in clear, concise language suitable to post as a GitHub PR comment.

Here is the diff:
{diff}";

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
            content = prompt,
            role = "user"
        }
    },
    model = "claude-sonnet-4-5-20250929"
};

var response = await client.PostAsync("https://api.anthropic.com/v1/messages", new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

var json = await response.Content.ReadAsStringAsync();
using var doc = JsonDocument.Parse(json);
var reviewText = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString() ?? "No response from claude";

var gh = new GitHubClient();
await gh.PostCommentAsync(reviewText);