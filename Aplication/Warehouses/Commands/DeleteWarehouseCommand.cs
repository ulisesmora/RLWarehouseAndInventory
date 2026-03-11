using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Warehouses.Commands
{
    public record DeleteWarehouseCommand(Guid Id) : IRequest;
}
