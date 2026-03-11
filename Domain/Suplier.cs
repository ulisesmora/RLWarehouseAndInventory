using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; }
        public string TaxId { get; set; }          // RFC, CIF, VAT
        public string Website { get; set; }

        // --- Datos de Contacto (Para automatización) ---
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }   // Vital para enviar correos automáticos
        public string ContactPhone { get; set; }

        // --- Configuración Financiera ---
        public string Currency { get; set; }       // "USD", "MXN"
        public int PaymentTermsDays { get; set; }  // Días de crédito (30, 60, 90)
        public bool IsActive { get; set; }

        // Relación: Lista de precios/materiales de este proveedor
        public virtual ICollection<SupplierMaterial> SupplierMaterials { get; set; }
    }
}
