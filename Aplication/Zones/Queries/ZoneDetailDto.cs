using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Queries
{
    public class ZoneDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Width { get; set; }
        public double Depth { get; set; }
        public double Height { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; }

        public bool AllowMixedLots { get; set; }

        // 2. Condiciones Ambientales
        public decimal? MinTemperatureCelsius { get; set; }
        public decimal? MaxTemperatureCelsius { get; set; }

        // 3. Seguridad (Tags permitidos)
        public List<string> AllowedHazmatTags { get; set; } = new List<string>();

        // 🔥 La lista de ubicaciones dentro de esta zona
        public List<ZoneBinDto> Bins { get; set; }
    }

    // DTO pequeño solo para mostrar en la lista de la zona
    public class ZoneBinDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } // "A-01-01"
        public decimal MaxWeight { get; set; }
        public decimal MaxVolume { get; set; }
    }
}
