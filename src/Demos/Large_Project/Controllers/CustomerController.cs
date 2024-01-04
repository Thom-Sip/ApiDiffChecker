using Large_Project.Models;
using Microsoft.AspNetCore.Mvc;

namespace Large_Project.Controllers
{
    [ApiController]
    [Route("{label}/[controller]")]
    public class CustomerController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ILogger<CustomerController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("{customerId}/account/{accountId}")]
        public async Task<Customer> Get(string label, int customerId, int accountId)
        {
            await Task.Delay(Random.Shared.Next(500, 5000));
            return Customer.FromIds(label, customerId, accountId);
        }

        [HttpGet]
        [Route("Find")]
        public async Task<List<Customer>> Find(string label, string name, int results)
        {
            await Task.Delay(Random.Shared.Next(500, 5000));
            return Customer.FindByName(label, name, results);
        }
    }
}
