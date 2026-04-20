using MediatR;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.WorkOrders.Commands
{
    public record CreateWorkOrderCommand(
    Guid ProductRecipeId,
    decimal PlannedQuantity,
    DateTime PlannedStartDate,
    string? Notes
) : IRequest<Guid>;
}
