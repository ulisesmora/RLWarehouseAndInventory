using MediatR;
using System;

namespace Inventory.Application.SalesOrders.Queries
{
    public record GetSalesOrderByIdQuery(Guid Id) : IRequest<SalesOrderDto>;
}
