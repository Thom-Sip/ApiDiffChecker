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
        [Route("customer/{customerId}")]
        public Customer Get(int customerId)
        {
            return new Customer
            {
                CustomerId = customerId,
                AccountId = 1,
                ContactDetails = new ContactDetails
                {
                    Name = "John",
                    Surname = "Doe",
                    Addresses =
                    [
                        new()
                        {
                            Street = "Molenstraat",
                            Country = "Nederland",
                            HouseNumber = 13,
                        },
                        new()
                        {
                            Street = "Riverstreet",
                            Country = "United Kingdom",
                            HouseNumber = 21,
                            HouseNumberSuffix = "C"
                        }
                    ]
                }
            };
        }
    }
}
