using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class StockItem : BaseEntity
    {
        public Guid WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; }

        public Guid MaterialId { get; set; }
        public virtual Material Material { get; set; }

        // --- Trazabilidad: ¿De qué lote específico? ---
        public Guid? LotId { get; set; }
        public virtual Lot Lot { get; set; }

        // --- Estado: ¿Está disponible o en cuarentena? ---
        public Guid StatusId { get; set; }
        public virtual StatusCatalog Status { get; set; }

        public Guid? StorageBinId { get; set; }
        public virtual StorageBin? StorageBin { get; set; }

        // --- Cantidades ---
        public decimal QuantityOnHand { get; set; }   // Lo que cuentas físicamente en el estante
        public decimal QuantityReserved { get; set; } // Lo que ya vendiste pero no has enviado

        // Propiedad calculada (útil para lógica, no se mapea necesariamente a BD)
        public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;
    }
}
