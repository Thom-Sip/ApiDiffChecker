using Large_Project.Model;
using Microsoft.AspNetCore.Mvc;

namespace Large_Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public Customer Get(int customerId, int accountId)
        {
            return Customer.FromIds(customerId, accountId);
        }
    }
}
