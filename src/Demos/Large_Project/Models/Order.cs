namespace Large_Project.Models
{
    public class Order
    {
        public string Id { get; set; }

        public string Label { get; set; }

        public DateTime OrderDate { get; set; }

        public List<OrderItem> Items { get; set; }

        public decimal ShippingCost { get; set; }

        public decimal TotalPrice { get; set; }

        public static Order GetOrder(string label, string orderId)
        {
            return new Order
            {
                Id = orderId,
                Label = label,
                OrderDate = DateTime.Today.AddDays(-31),
                ShippingCost = 14.99m,
                TotalPrice = 100m,
                Items =
                [
                    new OrderItem
                    {
                        Id = 87771,
                        Name = "Pen",
                        Description = "A nice pen",
                        Price = 15
                    }
                ]
            };
        }
    }

    public class OrderItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public Merchant Merchant { get; set; }
    }
}
