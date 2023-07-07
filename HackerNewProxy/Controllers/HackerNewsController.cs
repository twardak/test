using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace HackerNewProxy.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HackerNewsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public HackerNewsController(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Story>>> GetBestStories(int n)
        {
            try
            {
                var stories = await GetOrSetCachedStoryDetails(n);
                return Ok(stories);
            }
            catch()
            {
                return BadRequest();
            }
        }

        private async Task<IEnumerable<Story>> GetOrSetCachedStoryDetails(int n)
        {
            var allStoreis = await GetOrSetCachedBestStory();
            var result = allStoreis.OrderByDescending(o => o.Time).Take(n).OrderByDescending(n => n.Score);
            return result;
        }

        private async Task<IEnumerable<Story>> GetOrSetCachedBestStory()
        {
            var cacheKey = "BestStoryIds";
            if (!_cache.TryGetValue(cacheKey, out List<Story>? stories))
            {

                var storyIds = await GetAndDeserializeJson<int[]>("https://hacker-news.firebaseio.com/v0/beststories.json");
                if (storyIds != null && storyIds.Any())
                {
                    stories = new List<Story>();
                    foreach (var id in storyIds)
                    {
                        var story = await GetAndDeserializeJson<Story>($"https://hacker-news.firebaseio.com/v0/item/{id}.json");
                        if (story != null)
                        {
                            stories.Add(story);
                        }
                    }

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); // Cache for 5 minutes
                    _cache.Set(cacheKey, stories, cacheOptions);
                }
            }
            return stories ?? Enumerable.Empty<Story>();
        }

        private async Task<T?> GetAndDeserializeJson<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Ensure a successful response

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                
                PropertyNameCaseInsensitive = true // Optional: Ignore case sensitivity for property names
            };

            var result = JsonSerializer.Deserialize<T>(json, options);
            return result;
        }
    }
}
