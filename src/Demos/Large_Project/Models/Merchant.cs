using Large_Project.Model;

namespace Large_Project.Models
{
    public class Merchant
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Address MainOffice { get; set; }

        public List<Address> Stores { get; set; }
    }

    public class Store
    {
        public Guid Id { get; set; }

        public string LocationName { get; set; }

        public Address Address { get; set; }
    }
}
