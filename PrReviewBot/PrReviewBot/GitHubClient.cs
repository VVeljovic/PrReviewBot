using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PrReviewBot
{
    public class GitHubClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _repo;
        private readonly string _prNumber;

        public GitHubClient()
        {
            var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            _repo = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY")!;
            _prNumber = Environment.GetEnvironmentVariable("PR_NUMBER")!;

            if (string.IsNullOrEmpty(githubToken) || string.IsNullOrEmpty(_repo) || string.IsNullOrEmpty(_prNumber))
                throw new Exception("Missing GitHub environment variables.");

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PrReviewBot");
        }

        public async Task PostCommentAsync(string comment)
        {
            var payload = new
            {
                body = comment
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var url = $"https://api.github.com/repos/{_repo}/issues/{_prNumber}/comments";
            Console.WriteLine("Posting comment to: " + url);

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"Failed to post comment: {response.StatusCode}");
            else
                Console.WriteLine("Comment posted ");
        }
    }
}
