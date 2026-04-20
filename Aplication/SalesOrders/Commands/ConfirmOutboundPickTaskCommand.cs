using MediatR;
using System;

namespace Inventory.Application.SalesOrders.Commands
{
    public record ConfirmOutboundPickTaskCommand(
        Guid    SalesOrderId,
        Guid    TaskId,
        string  ScannedLpn,
        decimal PickedQuantity
    ) : IRequest<ConfirmOutboundPickResult>;

    public record ConfirmOutboundPickResult(
        bool   Success,
        string Message,
        bool   OrderReadyToShip,
        Guid?  NextTaskId
    );
}
