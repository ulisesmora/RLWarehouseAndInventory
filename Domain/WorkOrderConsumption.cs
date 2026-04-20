using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class WorkOrderConsumption : BaseTenantEntity
    {
        public Guid WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; } = null!;

        public Guid MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        // Para trazabilidad exacta:
        public Guid? LotId { get; set; }
        public Lot? Lot { get; set; }
        public Guid? SourceStorageBinId { get; set; } // De dónde se sacó en el Gemelo 3D



        public decimal PlannedQuantity { get; set; }
        public decimal ActualConsumedQuantity { get; set; }
        public decimal ScrapQuantity => ActualConsumedQuantity - PlannedQuantity; // Merma
    }
}
