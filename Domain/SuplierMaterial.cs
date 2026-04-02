using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class SupplierMaterial : BaseTenantEntity
    {
        public Guid SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        public Guid MaterialId { get; set; }
        public virtual Material Material { get; set; }

        // --- Datos Logísticos del Proveedor ---
        public string VendorSKU { get; set; }      // El código que usa el proveedor (SKU Externo)
        public string VendorMaterialName { get; set; }

        // --- Economía ---
        public decimal UnitCost { get; set; }      // Costo pactado
        public string Currency { get; set; }

        // --- Reglas para el Bot de Compras ---
        public decimal MinimumOrderQuantity { get; set; } // "MOQ"
        public int LeadTimeDays { get; set; }      // Tiempo de entrega estimado

        public bool IsPreferred { get; set; }
    }
}
