using MediatR;
using System;

namespace Inventory.Application.SalesOrders.Commands
{
    public record CancelSalesOrderCommand(Guid SalesOrderId, string? Reason) : IRequest<Unit>;
}
