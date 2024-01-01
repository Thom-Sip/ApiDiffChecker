using Large_Project.Models;

namespace Large_Project.Model
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public int AccountId { get; set; }

        public ContactDetails ContactDetails { get; set; }

        public List<Order> Orders { get; set; } = [];

        public Guid TraceId { get; set; } = Guid.NewGuid();

        public static Customer FromIds(int customerId, int accountId)
        {
            return new Customer
            {
                CustomerId = customerId,
                AccountId = accountId,
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

    public class ContactDetails
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public List<Address> Addresses { get; set; } = [];
    }

    public class Address
    {
        public string Street { get; set; }

        public int HouseNumber { get; set; }

        public string HouseNumberSuffix { get; set; }

        public string Country { get; set; }
    }
}
