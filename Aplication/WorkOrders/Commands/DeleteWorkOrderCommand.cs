using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.WorkOrders.Commands
{
    public record DeleteWorkOrderCommand(Guid Id) : IRequest<Unit>;
}
