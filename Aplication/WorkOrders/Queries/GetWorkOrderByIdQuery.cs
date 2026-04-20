using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.WorkOrders.Queries
{
    public record GetWorkOrderByIdQuery(Guid Id) : IRequest<WorkOrderDto>;
}
