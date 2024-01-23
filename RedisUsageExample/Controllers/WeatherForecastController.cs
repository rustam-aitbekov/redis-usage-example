using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace RedisUsageExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        readonly DistributedCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10),
        };

        private readonly IDistributedCache _cache;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            string cacheKey = "WeatherForecast";

            string? itemsCached = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrWhiteSpace(itemsCached))
            {
                return JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(itemsCached);
            }

            var items = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            var jsonData = JsonSerializer.Serialize(items);

            _cache.SetString(cacheKey, jsonData, options);

            return items;
        }
    }
}