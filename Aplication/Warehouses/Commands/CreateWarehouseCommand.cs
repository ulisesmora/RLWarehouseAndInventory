using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Warehouses.Commands
{
    
    public record CreateWarehouseCommand(
        string Name,
        string? Location,
        bool? IsMain,
        decimal? Capacity
    ) : IRequest<Guid>;
    
}
