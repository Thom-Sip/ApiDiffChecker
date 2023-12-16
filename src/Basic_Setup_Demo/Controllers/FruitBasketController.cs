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

        [HttpGet("{customerId}/orders/{orderId}")]
        public IEnumerable<string> GetThings(int customerId, string orderId, [FromQuery]string message = "", [FromQuery] string key = "")
        {
            return new List<string>
            {
                $"Customer_{customerId}",
                $"Order_{orderId}",
                $"Message_{message}",
                $"Key_{key}",
                Summaries[Random.Shared.Next(Summaries.Length)],
                Summaries[Random.Shared.Next(Summaries.Length)],
                Summaries[Random.Shared.Next(Summaries.Length)]
            };
        }
    }
}
