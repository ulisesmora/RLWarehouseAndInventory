using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Queries
{
    public class ZoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Dimensiones para el 3D
        public double Width { get; set; }
        public double Depth { get; set; }
        public double Height { get; set; }

        public bool AllowMixedLots { get; set; }

        // 2. Condiciones Ambientales
        public decimal? MinTemperatureCelsius { get; set; }
        public decimal? MaxTemperatureCelsius { get; set; }

        // 3. Seguridad (Tags permitidos)
        public List<string> AllowedHazmatTags { get; set; } = new List<string>();

        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } // Flattening
    }
}
