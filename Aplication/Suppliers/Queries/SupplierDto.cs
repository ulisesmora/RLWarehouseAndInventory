using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Suppliers.Queries
{
    public class SupplierDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TaxId { get; set; } // RFC/CIF/NIT
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }
}
