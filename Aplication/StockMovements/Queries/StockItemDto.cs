using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Queries
{
    public class StockItemDto
    {
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; }

        public Guid? StorageBinId { get; set; }
        public string? StorageBinCode { get; set; }

        public Guid? LotId { get; set; }
        public string? LotNumber { get; set; }

        public Guid StatusId { get; set; }
        public string StatusName { get; set; } // Asumiendo que tu StatusCatalog tiene un Name

        // Las cantidades clave
        public decimal QuantityOnHand { get; set; }
        public decimal QuantityReserved { get; set; }
        public decimal QuantityAvailable { get; set; }
    }
}
