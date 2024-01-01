namespace Large_Project.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public DateTime OrderData { get; set; }

        public List<OrderItem> Items { get; set; }

        public decimal ShippingCost { get; set; }

        public decimal TotalPrice { get; set; }
    }

    public class OrderItem
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public Merchant Merchant { get; set; }
    }
}
