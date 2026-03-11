using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Warehouses.Queries
{
    public class WarehouseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }

        public decimal Capacity { get; set; }
        public bool IsMain { get; set; }

        // Información útil para el frontend
        public int ZonesCount { get; set; }
    }
}
