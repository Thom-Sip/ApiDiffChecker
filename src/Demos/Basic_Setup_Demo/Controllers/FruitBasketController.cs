using Microsoft.AspNetCore.Mvc;

namespace Basic_Setup_Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FruitBasketController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Apple", "Pear", "Orange", "Strawberry"
        };

        private readonly ILogger<FruitBasketController> _logger;

        public FruitBasketController(ILogger<FruitBasketController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{customerId}/{accountId}")]
        public async Task <IEnumerable<string>> GetThings(int customerId, int accountId, [FromQuery] string message = "")
        {
            await Task.Delay(Random.Shared.Next(500, 5000));

            return new List<string>
            {
                $"Customer_{customerId}",
                $"Customer_{accountId}",
                $"Message_{message}",
                Summaries[Random.Shared.Next(Summaries.Length)],
                Summaries[Random.Shared.Next(Summaries.Length)],
            };
        }

        [HttpGet("random")]
        public async Task<IEnumerable<string>> RandomResult()
        {
            await Task.Delay(Random.Shared.Next(500, 5000));

            return new List<string>
            {
                $"Random_{Random.Shared.Next(2)}",
            };
        }
    }
}
