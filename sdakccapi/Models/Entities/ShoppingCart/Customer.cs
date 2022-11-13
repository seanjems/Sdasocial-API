
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sdakccapi.Models.Entities
{
    public class Customer
    {
        [Key]
        public long CustomerID { get; set; }
        public Guid? AccountNumber { get; set; }
        public string? FullName { get; set; }
        public string? Contact { get; set; }
        public string? CardNumber { get; set; }
        public string? VatNumber { get; set; }
        public string? Email { get; set; }
        public string? CountryCode { get; set; }
        public long? AddressID { get; set; }
        public decimal? creditLimit { get; set; }
        public bool? deleted { get; set; }
        public string? Company { get; set; }

        public Customer()
        {
            AccountNumber = Guid.NewGuid();
        }

    }
}
