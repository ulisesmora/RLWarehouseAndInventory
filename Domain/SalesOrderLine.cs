using Domain;
using System;

namespace Inventory.Domain
{
    public class SalesOrderLine : BaseTenantEntity
    {
        public Guid SalesOrderId { get; set; }
        public virtual SalesOrder SalesOrder { get; set; } = null!;

        /// <summary>
        /// Nullable: puede ser null cuando la línea viene de un canal externo
        /// (WooCommerce, Shopify, ML) y aún no tiene un material interno asignado.
        /// Una vez que el usuario mapea el producto en el Integration Hub, se actualiza.
        /// </summary>
        public Guid? MaterialId { get; set; }
        public virtual Material? Material { get; set; }

        /// <summary>Nombre del producto externo (guardado para mostrar aunque no haya material mapeado)</summary>
        public string? ExternalProductName { get; set; }

        /// <summary>SKU externo original del canal de venta</summary>
        public string? ExternalSku { get; set; }

        /// <summary>Cantidad pedida por el cliente</summary>
        public decimal OrderedQuantity { get; set; }

        /// <summary>Precio unitario acordado</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>Se actualiza a medida que se confirman OutboundPickTasks</summary>
        public decimal PickedQuantity { get; set; } = 0;

        public SalesOrderLineStatus Status { get; set; } = SalesOrderLineStatus.Pending;
    }
}