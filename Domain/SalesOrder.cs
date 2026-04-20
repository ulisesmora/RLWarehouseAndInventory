using Domain;
using System;
using System.Collections.Generic;

namespace Inventory.Domain
{
    public class SalesOrder : BaseTenantEntity
    {
        /// <summary>Número auto-generado: SO-{yyyyMMdd}-{4chars}</summary>
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>Canal de origen del pedido</summary>
        public SalesChannel SourceChannel { get; set; } = SalesChannel.Manual;

        /// <summary>Nombre del cliente o empresa</summary>
        public string CustomerName { get; set; } = string.Empty;

        public string? CustomerEmail { get; set; }

        /// <summary>Dirección de envío (texto libre; se estructurará al integrar carriers)</summary>
        public string? ShippingAddress { get; set; }

        public DateTime? ShipByDate { get; set; }

        public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;

        public string? Notes { get; set; }

        /// <summary>Referencia externa del canal (ej. ID de MeLi, WooCommerce order ID)</summary>
        public string? ExternalReference { get; set; }

        public virtual ICollection<SalesOrderLine>    Lines     { get; set; } = new List<SalesOrderLine>();
        public virtual ICollection<OutboundPickTask>  PickTasks { get; set; } = new List<OutboundPickTask>();
    }
}
