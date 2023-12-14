using Microsoft.AspNetCore.Mvc;

namespace Sample_Api_Demo.Controllers
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

        [HttpGet("GetThreeFruits/{customerId}/things")]
        public IEnumerable<string> GetThings(int customerId)
        {
            return Enumerable.Range(1, 3).Select(index => Summaries[Random.Shared.Next(Summaries.Length)]).ToArray();
        }
    }
}
