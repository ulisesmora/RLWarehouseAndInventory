using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Warehouses.Commands
{
    public record UpdateWarehouseCommand(
        Guid Id,
        string Name,
        string? Location,
        bool? IsMain,
        decimal? Capacity
    ) : IRequest;
}
