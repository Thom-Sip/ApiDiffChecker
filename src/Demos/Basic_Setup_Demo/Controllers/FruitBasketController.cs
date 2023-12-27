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
        public IEnumerable<string> GetThings(int customerId, int accountId, [FromQuery] string message = "")
        {
            return new List<string>
            {
                $"Customer_{customerId}",
                $"Customer_{accountId}",
                $"Message_{message}",
                Summaries[Random.Shared.Next(Summaries.Length)],
                Summaries[Random.Shared.Next(Summaries.Length)],
                Summaries[Random.Shared.Next(Summaries.Length)]
            };
        }

        [HttpGet("random")]
        public IEnumerable<string> RandomResult()
        {
            return new List<string>
            {
                $"Random_{Random.Shared.Next(2)}",
            };
        }
    }
}
