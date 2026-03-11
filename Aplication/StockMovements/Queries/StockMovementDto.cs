using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Queries
{
    public class StockMovementDto
    {
        public Guid Id { get; set; }
        public DateTime MovementDate { get; set; }

        public string Type { get; set; } // Lo pasaremos a texto ("SalesShipment", etc.)
        public decimal Quantity { get; set; } // Recordar: Positivo = Entrada, Negativo = Salida

        public string WarehouseName { get; set; }
        public string? StorageBinCode { get; set; }
        public string? LotNumber { get; set; }

        public string ReferenceNumber { get; set; }
        public string Comments { get; set; }
        public Guid UserId { get; set; }
    }
}
