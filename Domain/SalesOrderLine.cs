using Domain;
using System;

namespace Inventory.Domain
{
    public class SalesOrderLine : BaseTenantEntity
    {
        public Guid SalesOrderId { get; set; }
        public virtual SalesOrder SalesOrder { get; set; } = null!;

        public Guid MaterialId { get; set; }
        public virtual Material Material { get; set; } = null!;

        /// <summary>Cantidad pedida por el cliente</summary>
        public decimal OrderedQuantity { get; set; }

        /// <summary>Precio unitario acordado</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>Se actualiza a medida que se confirman OutboundPickTasks</summary>
        public decimal PickedQuantity { get; set; } = 0;

        public SalesOrderLineStatus Status { get; set; } = SalesOrderLineStatus.Pending;
    }
}
