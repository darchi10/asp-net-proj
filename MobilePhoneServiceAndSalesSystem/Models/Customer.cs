using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Phone> Phones { get; set; } = new List<Phone>();
    }
}
