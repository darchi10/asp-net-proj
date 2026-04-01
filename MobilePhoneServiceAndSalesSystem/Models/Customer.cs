using System.Collections.Generic;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Phone> Phones { get; set; } = new List<Phone>();
    }
}
