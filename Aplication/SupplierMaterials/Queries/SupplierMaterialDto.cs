using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.SupplierMaterials.Queries
{
    public class SupplierMaterialDto
    {
        public Guid Id { get; set; }

        // Relaciones aplanadas
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; }

        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; }

        // Datos logísticos y económicos
        public string VendorSKU { get; set; }
        public string VendorMaterialName { get; set; }
        public decimal UnitCost { get; set; }
        public string Currency { get; set; }
        public decimal MinimumOrderQuantity { get; set; }
        public int LeadTimeDays { get; set; }
        public bool IsPreferred { get; set; }
    }
}
