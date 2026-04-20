using MediatR;
using System;

namespace Inventory.Application.SalesOrders.Commands
{
    public record ShipSalesOrderCommand(
        Guid    SalesOrderId,
        string? TrackingNumber,
        string? CarrierName,
        string? Notes
    ) : IRequest<Unit>;
}
