using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.WorkOrders.Commands
{
    public record UpdateWorkOrderCommand(
     Guid Id,
     DateTime PlannedStartDate,
     string? Notes
 ) : IRequest<Unit>;
}
