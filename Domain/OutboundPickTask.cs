using Domain;
using System;

namespace Inventory.Domain
{
    /// <summary>
    /// Tarea de picking para una orden de venta. Apunta al StockItem exacto
    /// que el operador debe tomar del almacén (asignado por FEFO en la creación).
    /// </summary>
    public class OutboundPickTask : BaseTenantEntity
    {
        public Guid SalesOrderId { get; set; }
        public virtual SalesOrder SalesOrder { get; set; } = null!;

        public Guid SalesOrderLineId { get; set; }
        public virtual SalesOrderLine SalesOrderLine { get; set; } = null!;

        public Guid MaterialId { get; set; }
        public virtual Material Material { get; set; } = null!;

        /// <summary>StockItem asignado por FEFO — la caja/pallet exacta a recoger</summary>
        public Guid SourceStockItemId { get; set; }
        public virtual StockItem SourceStockItem { get; set; } = null!;

        public decimal RequiredQuantity { get; set; }
        public decimal PickedQuantity   { get; set; } = 0;

        public PickTaskStatus Status { get; set; } = PickTaskStatus.Pending;
    }
}
