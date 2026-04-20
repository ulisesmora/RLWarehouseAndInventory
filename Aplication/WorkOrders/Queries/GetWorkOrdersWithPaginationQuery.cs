using Inventory.Application.Materials.Commons.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.WorkOrders.Queries
{
    public record GetWorkOrdersWithPaginationQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null
) : IRequest<PaginatedList<WorkOrderDto>>;
}
