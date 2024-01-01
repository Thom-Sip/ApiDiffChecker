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
        public Order Get(string label, string orderId)
        {
            return Order.GetOrder(label, orderId);
        }

        [HttpGet]
        [Route("Find")]
        public List<Customer> Find(string label, string name, int results)
        {
            return Customer.FindByName(label, name, results);
        }
    }
}
