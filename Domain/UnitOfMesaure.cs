using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class UnitOfMeasure : BaseTenantEntity
    {
        public string Name { get; set; }        // Ej: "Kilogramo", "Metro", "Unidad"
        public string Abbreviation { get; set; } // Ej: "kg", "m", "u"

        // Opcional: Si quieres conversiones base (ej: 1000g = 1kg)
        public bool IsBaseUnit { get; set; }
        public decimal ConversionFactor { get; set; }
    }
}
