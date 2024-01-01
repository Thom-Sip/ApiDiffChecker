using Large_Project.Models;

namespace Large_Project.Model
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public int AccountId { get; set; }

        public ContactDetails ContactDetails { get; set; }

        public List<Order> Orders { get; set; } = [];
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
