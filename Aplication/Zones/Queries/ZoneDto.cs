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

        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } // Flattening
    }
}
