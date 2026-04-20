using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
   

    public class ProductionPickTask : BaseTenantEntity
    {
        public Guid WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; } = null!;

        public Guid MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        // 🔥 AQUÍ ESTÁ EL ENLACE EXACTO AL GEMELO 3D Y AL LOTE
        public Guid SourceStockItemId { get; set; }
        public StockItem SourceStockItem { get; set; } = null!;

        public decimal RequiredQuantity { get; set; }
        public decimal PickedQuantity { get; set; } // Lo que el operador realmente agarró

        public PickTaskStatus Status { get; set; } = PickTaskStatus.Pending;
    }
}
