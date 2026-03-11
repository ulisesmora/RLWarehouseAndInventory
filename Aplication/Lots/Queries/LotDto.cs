using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Lots.Queries
{
    public class LotDto
    {
        public Guid Id { get; set; }
        public string LotNumber { get; set; }        // Lote #12345
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpirationDate { get; set; }

        // Relación
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; }

        // 🔥 Dato Calculado: ¿Cuánto queda de este lote en TOTAL (sumando todas las bodegas)?
        public decimal TotalQuantity { get; set; }

        public decimal QuantityOnHand { get; set; }
        public decimal QuantityAvailable { get; set; }
        public decimal QuantityReserved { get; set; }


        public decimal InitialReceivedQuantity { get; set; }

        public bool IsBlocked { get; set; }

        public string? SupplierName { get; set; }

        public string UnitName { get; set; }

        public Guid? SupplierId { get; set; }





        // Estado (Calculado en base a fecha)
        public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;
    }
}
