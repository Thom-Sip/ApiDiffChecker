using Large_Project.Models;
using Microsoft.AspNetCore.Mvc;

namespace Large_Project.Controllers
{
    [ApiController]
    [Route("{label}/[controller]")]
    public class OrdersController : ControllerBase
    {
        [HttpGet]
        [Route("{orderId}")]
        public async Task<Order> Get(string label, string orderId)
        {
            await Task.Delay(Random.Shared.Next(500, 5000));
            return Order.GetOrder(label, orderId);
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
