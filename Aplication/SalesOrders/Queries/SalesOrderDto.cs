using System;
using System.Collections.Generic;

namespace Inventory.Application.SalesOrders.Queries
{
    public class SalesOrderDto
    {
        public Guid   Id               { get; set; }
        public string OrderNumber      { get; set; } = string.Empty;
        public string SourceChannel    { get; set; } = string.Empty;
        public string CustomerName     { get; set; } = string.Empty;
        public string? CustomerEmail   { get; set; }
        public string? ShippingAddress { get; set; }
        public DateTime? ShipByDate    { get; set; }
        public string Status           { get; set; } = string.Empty;
        public string? Notes           { get; set; }
        public string? ExternalReference { get; set; }
        public DateTime CreatedAt      { get; set; }

        public List<SalesOrderLineDto>   Lines     { get; set; } = new();
        public List<OutboundPickTaskDto> PickTasks { get; set; } = new();
    }

    public class SalesOrderLineDto
    {
        public Guid    Id               { get; set; }
        public Guid    MaterialId       { get; set; }
        public string  MaterialName     { get; set; } = string.Empty;
        public decimal OrderedQuantity  { get; set; }
        public decimal PickedQuantity   { get; set; }
        public decimal UnitPrice        { get; set; }
        public string  Status           { get; set; } = string.Empty;
    }

    public class OutboundPickTaskDto
    {
        public Guid    Id                 { get; set; }
        public Guid    SalesOrderLineId   { get; set; }
        public Guid    MaterialId         { get; set; }
        public string  MaterialName       { get; set; } = string.Empty;
        public Guid    SourceStockItemId  { get; set; }
        public string  LpnCode           { get; set; } = string.Empty;
        public string  LotNumber         { get; set; } = string.Empty;
        public string  BinCode           { get; set; } = string.Empty;
        public string  ZoneName          { get; set; } = string.Empty;
        public string  LocationLabel     { get; set; } = string.Empty;
        public decimal RequiredQuantity  { get; set; }
        public decimal PickedQuantity    { get; set; }
        public string  Status            { get; set; } = string.Empty;
    }
}
