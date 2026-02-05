using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PrReviewBot
{
    public class GitHubClient
    {
        private readonly HttpClient _client;
        private readonly string _repo;
        private readonly string _prNumber;

        public GitHubClient()
        {
            var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            _repo = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY")!;
            _prNumber = Environment.GetEnvironmentVariable("GITHUB_REF")?.Split('/').Last()!;

            if (string.IsNullOrEmpty(githubToken) || string.IsNullOrEmpty(_repo) || string.IsNullOrEmpty(_prNumber))
                throw new Exception("Missing GitHub environment variables.");

            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
            _client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            _client.DefaultRequestHeaders.Add("User-Agent", "Claude-Bot");
        }

        public async Task PostCommentAsync(string comment)
        {
            var payload = new
            {
                body = comment
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(
                $"https://api.github.com/repos/{_repo}/issues/{_prNumber}/comments",
                content
            );

            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"Failed to post comment: {response.StatusCode}");
            else
                Console.WriteLine("Comment posted ✅");
        }
    }
}
