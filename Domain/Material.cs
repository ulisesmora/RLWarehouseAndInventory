using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class Material : BaseTenantEntity
    {
        public string Name { get; set; }
        public string SKU { get; set; }            // Tu código interno
        public string? BarCode { get; set; }        // EAN/UPC escaneable
        public string? Description { get; set; }

        public MaterialType Type { get; set; }

        // --- Logística ---
        public decimal? Weight { get; set; }
        public decimal? Volume { get; set; }
        public bool IsStockable { get; set; }

        // --- Planificación de Stock ---
        public decimal ReorderPoint { get; set; }  // Stock mínimo (Alerta)
        public decimal TargetStock { get; set; }   // Stock ideal

        public decimal StandardCost { get; set; }
        public decimal SalesPrice { get; set; }

        // --- Relaciones ---
        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public Guid UnitOfMeasureId { get; set; }
        public virtual UnitOfMeasure UnitOfMeasure { get; set; }

        public bool? IsFragile { get; set; }
        public int? MaxStackingLayers { get; set; } = 1; // Por defecto no se apila (1 capa)

        // --- 2. Condiciones Ambientales ---
        // Usamos nullable (decimal?) porque una llanta o un tornillo no tienen límite de temperatura
        public decimal? MinTemperatureCelsius { get; set; }
        public decimal? MaxTemperatureCelsius { get; set; }

        // --- 3. Seguridad y Normativas (TAGS) ---
        // Guardaremos una lista de etiquetas: ["Flammable", "Corrosive", "Toxic"]
        public List<string> HazmatTags { get; set; } = new List<string>();

        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }

        public decimal? VolumeCm3 => (LengthCm.HasValue && WidthCm.HasValue && HeightCm.HasValue)
                                     ? (LengthCm * WidthCm * HeightCm)
                                     : null;

        // Colección de proveedores que venden este material
        public virtual ICollection<SupplierMaterial> SupplierMaterials { get; set; }

        // Lotes existentes de este material
        public virtual ICollection<Lot> Lots { get; set; }

        public virtual ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
    }
}
